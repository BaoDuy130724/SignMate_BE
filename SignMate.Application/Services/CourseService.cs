using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class CourseService : ICourseService
{
    private readonly ISignMateDbContext _db;

    public CourseService(ISignMateDbContext db) => _db = db;

    public async Task<List<CourseDto>> GetCoursesAsync(string? search, string? level, bool includeUnpublished = false)
    {
        var query = _db.Courses.AsNoTracking().AsQueryable();

        if (!includeUnpublished)
            query = query.Where(c => c.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || (c.Description != null && c.Description.Contains(search)));

        if (!string.IsNullOrWhiteSpace(level) && Enum.TryParse<CourseLevel>(level, true, out var lvl))
            query = query.Where(c => c.Level == lvl);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CourseDto
            {
                Id = c.Id, Title = c.Title, Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl, Level = c.Level.ToString(),
                IsPublished = c.IsPublished, CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt, LessonCount = c.Lessons.Count
            })
            .ToListAsync();
    }

    public async Task<CourseDetailDto?> GetCourseByIdAsync(Guid id)
    {
        return await _db.Courses
            .AsNoTracking()
            .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Where(c => c.Id == id)
            .Select(c => new CourseDetailDto
            {
                Id = c.Id, Title = c.Title, Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl, Level = c.Level.ToString(),
                IsPublished = c.IsPublished, CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt, LessonCount = c.Lessons.Count,
                Lessons = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => new LessonDto
                {
                    Id = l.Id, CourseId = l.CourseId, Title = l.Title,
                    OrderIndex = l.OrderIndex, VideoUrl = l.VideoUrl,
                    Description = l.Description, DurationSeconds = l.DurationSeconds,
                    IsPublished = l.IsPublished, SignCount = l.Signs.Count
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, Guid createdBy)
    {
        if (!Enum.TryParse<CourseLevel>(request.Level, true, out var level))
            throw new ArgumentException($"Invalid level: {request.Level}");

        var course = new Course
        {
            Id = Guid.NewGuid(), Title = request.Title, Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl, Level = level,
            IsPublished = false, CreatedBy = createdBy, CreatedAt = DateTime.UtcNow
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id, Title = course.Title, Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl, Level = course.Level.ToString(),
            IsPublished = course.IsPublished, CreatedBy = course.CreatedBy,
            CreatedAt = course.CreatedAt, LessonCount = 0
        };
    }

    public async Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseRequest request)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return null;

        if (request.Title != null) course.Title = request.Title;
        if (request.Description != null) course.Description = request.Description;
        if (request.ThumbnailUrl != null) course.ThumbnailUrl = request.ThumbnailUrl;
        if (request.IsPublished.HasValue) course.IsPublished = request.IsPublished.Value;
        if (request.Level != null && Enum.TryParse<CourseLevel>(request.Level, true, out var lvl))
            course.Level = lvl;

        await _db.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id, Title = course.Title, Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl, Level = course.Level.ToString(),
            IsPublished = course.IsPublished, CreatedBy = course.CreatedBy,
            CreatedAt = course.CreatedAt,
            LessonCount = await _db.Lessons.CountAsync(l => l.CourseId == id)
        };
    }

    public async Task DeleteCourseAsync(Guid id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course != null)
        {
            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId)
    {
        return await _db.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonDto
            {
                Id = l.Id, CourseId = l.CourseId, Title = l.Title,
                OrderIndex = l.OrderIndex, VideoUrl = l.VideoUrl,
                Description = l.Description, DurationSeconds = l.DurationSeconds,
                IsPublished = l.IsPublished, SignCount = l.Signs.Count
            })
            .ToListAsync();
    }

    public async Task<LessonDetailDto?> GetLessonByIdAsync(Guid lessonId)
    {
        return await _db.Lessons
            .AsNoTracking()
            .Include(l => l.Signs.OrderBy(s => s.OrderIndex))
            .Where(l => l.Id == lessonId)
            .Select(l => new LessonDetailDto
            {
                Id = l.Id, CourseId = l.CourseId, Title = l.Title,
                OrderIndex = l.OrderIndex, VideoUrl = l.VideoUrl, Topic = l.Topic,
                Description = l.Description, DurationSeconds = l.DurationSeconds,
                IsPublished = l.IsPublished, SignCount = l.Signs.Count,
                Signs = l.Signs.OrderBy(s => s.OrderIndex).Select(s => new SignDto
                {
                    Id = s.Id, Word = s.Word, VideoUrl = s.VideoUrl,
                    ThumbnailUrl = s.ThumbnailUrl, Description = s.Description,
                    OrderIndex = s.OrderIndex, ReferenceKeypointData = s.ReferenceKeypointData
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LessonDto> CreateLessonAsync(Guid courseId, CreateLessonRequest request)
    {
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(), CourseId = courseId, Title = request.Title,
            Topic = request.Topic, OrderIndex = request.OrderIndex,
            VideoUrl = request.VideoUrl, Description = request.Description,
            DurationSeconds = request.DurationSeconds, IsPublished = false
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id, CourseId = lesson.CourseId, Title = lesson.Title, Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex, VideoUrl = lesson.VideoUrl, Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds, IsPublished = lesson.IsPublished, SignCount = 0
        };
    }

    public async Task<LessonDto?> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson == null) return null;

        if (request.Title != null) lesson.Title = request.Title;
        if (request.Topic != null) lesson.Topic = request.Topic;
        if (request.OrderIndex.HasValue) lesson.OrderIndex = request.OrderIndex.Value;
        if (request.VideoUrl != null) lesson.VideoUrl = request.VideoUrl;
        if (request.Description != null) lesson.Description = request.Description;
        if (request.DurationSeconds.HasValue) lesson.DurationSeconds = request.DurationSeconds.Value;
        if (request.IsPublished.HasValue) lesson.IsPublished = request.IsPublished.Value;

        await _db.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id, CourseId = lesson.CourseId, Title = lesson.Title, Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex, VideoUrl = lesson.VideoUrl, Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds, IsPublished = lesson.IsPublished,
            SignCount = await _db.SignProgresses.CountAsync(sp => sp.Sign.LessonId == lessonId) // Approximate or just signs table
        };
    }

    public async Task DeleteLessonAsync(Guid lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if (lesson != null)
        {
            _db.Lessons.Remove(lesson);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> SetSignReferenceAsync(SetReferenceRequest request)
    {
        var sign = await _db.Signs.FindAsync(request.SignId);
        if (sign == null) return false;

        sign.ReferenceKeypointData = request.ReferenceKeypointData;
        await _db.SaveChangesAsync();
        return true;
    }
}
