using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<CourseDto>> GetCoursesAsync(string? search, string? level, bool includeUnpublished = false)
    {
        var query = _unitOfWork.Repository<Course>().Query().AsNoTracking();

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
                CreatedAt = c.CreatedAt, LessonCount = c.Lessons.Count,
                Topic = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => l.Topic).FirstOrDefault() ?? "Chung"
            })
            .ToListAsync();
    }

    public async Task<CourseDetailDto?> GetCourseByIdAsync(int id)
    {
        return await _unitOfWork.Repository<Course>().Query()
            .AsNoTracking()
            .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Where(c => c.Id == id)
            .Select(c => new CourseDetailDto
            {
                Id = c.Id, Title = c.Title, Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl, Level = c.Level.ToString(),
                IsPublished = c.IsPublished, CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt, LessonCount = c.Lessons.Count,
                Topic = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => l.Topic).FirstOrDefault() ?? "Chung",
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

    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, int createdBy)
    {
        if (!Enum.TryParse<CourseLevel>(request.Level, true, out var level))
            throw new ArgumentException($"Invalid level: {request.Level}");

        var course = new Course
        {
            Id = 0, Title = request.Title, Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl, Level = level,
            IsPublished = false, CreatedBy = createdBy, CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Course>().AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id, Title = course.Title, Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl, Level = course.Level.ToString(),
            IsPublished = course.IsPublished, CreatedBy = course.CreatedBy,
            CreatedAt = course.CreatedAt, LessonCount = 0, Topic = "Chung"
        };
    }

    public async Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseRequest request)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id);
        if (course == null) return null;

        if (request.Title != null) course.Title = request.Title;
        if (request.Description != null) course.Description = request.Description;
        if (request.ThumbnailUrl != null) course.ThumbnailUrl = request.ThumbnailUrl;
        if (request.IsPublished.HasValue) course.IsPublished = request.IsPublished.Value;
        if (request.Level != null && Enum.TryParse<CourseLevel>(request.Level, true, out var lvl))
            course.Level = lvl;

        await _unitOfWork.SaveChangesAsync();

        var topic = await _unitOfWork.Repository<Lesson>().Query()
            .Where(l => l.CourseId == id)
            .OrderBy(l => l.OrderIndex)
            .Select(l => l.Topic)
            .FirstOrDefaultAsync() ?? "Chung";

        return new CourseDto
        {
            Id = course.Id, Title = course.Title, Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl, Level = course.Level.ToString(),
            IsPublished = course.IsPublished, CreatedBy = course.CreatedBy,
            CreatedAt = course.CreatedAt,
            LessonCount = await _unitOfWork.Repository<Lesson>().Query().CountAsync(l => l.CourseId == id),
            Topic = topic
        };
    }

    public async Task DeleteCourseAsync(int id)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id);
        if (course != null)
        {
            _unitOfWork.Repository<Course>().Delete(course);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<LessonDto>> GetLessonsByCourseAsync(int courseId)
    {
        return await _unitOfWork.Repository<Lesson>().Query()
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

    public async Task<LessonDetailDto?> GetLessonByIdAsync(int lessonId)
    {
        return await _unitOfWork.Repository<Lesson>().Query()
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

    public async Task<LessonDto> CreateLessonAsync(int courseId, CreateLessonRequest request)
    {
        var lesson = new Lesson
        {
            Id = 0, CourseId = courseId, Title = request.Title,
            Topic = request.Topic, OrderIndex = request.OrderIndex,
            VideoUrl = request.VideoUrl, Description = request.Description,
            DurationSeconds = request.DurationSeconds, IsPublished = false
        };
        await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id, CourseId = lesson.CourseId, Title = lesson.Title, Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex, VideoUrl = lesson.VideoUrl, Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds, IsPublished = lesson.IsPublished, SignCount = 0
        };
    }

    public async Task<LessonDto?> UpdateLessonAsync(int lessonId, UpdateLessonRequest request)
    {
        var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(lessonId);
        if (lesson == null) return null;

        if (request.Title != null) lesson.Title = request.Title;
        if (request.Topic != null) lesson.Topic = request.Topic;
        if (request.OrderIndex.HasValue) lesson.OrderIndex = request.OrderIndex.Value;
        if (request.VideoUrl != null) lesson.VideoUrl = request.VideoUrl;
        if (request.Description != null) lesson.Description = request.Description;
        if (request.DurationSeconds.HasValue) lesson.DurationSeconds = request.DurationSeconds.Value;
        if (request.IsPublished.HasValue) lesson.IsPublished = request.IsPublished.Value;

        await _unitOfWork.SaveChangesAsync();

        return new LessonDto
        {
            Id = lesson.Id, CourseId = lesson.CourseId, Title = lesson.Title, Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex, VideoUrl = lesson.VideoUrl, Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds, IsPublished = lesson.IsPublished,
            SignCount = await _unitOfWork.Repository<Sign>().Query().CountAsync(s => s.LessonId == lessonId)
        };
    }

    public async Task DeleteLessonAsync(int lessonId)
    {
        var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(lessonId);
        if (lesson != null)
        {
            _unitOfWork.Repository<Lesson>().Delete(lesson);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<bool> SetSignReferenceAsync(SetReferenceRequest request)
    {
        var sign = await _unitOfWork.Repository<Sign>().GetByIdAsync(request.SignId);
        if (sign == null) return false;

        sign.ReferenceKeypointData = request.ReferenceKeypointData;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
