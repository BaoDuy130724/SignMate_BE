using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Dashboard;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ISignMateDbContext _db;

    public DashboardService(ISignMateDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        var streak = await _db.Streaks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        // Compute average directly on DB instead of loading all attempts into memory
        double avgAcc = await _db.PracticeAttempts
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;
        avgAcc *= 100;

        // Suggested Lesson logic
        var suggested = await _db.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsPublished);

        LessonDto? lessonDto = suggested == null ? null : new LessonDto
        {
            Id = suggested.Id, CourseId = suggested.CourseId, Title = suggested.Title,
            Description = suggested.Description, DurationSeconds = suggested.DurationSeconds,
            Topic = suggested.Topic
        };

        // Fetch Student's classes to get assignments
        var classIds = await _db.ClassStudents
            .Where(cs => cs.StudentId == userId)
            .Select(cs => cs.ClassId)
            .ToListAsync();

        var assignments = await _db.LessonAssignments
            .AsNoTracking()
            .Where(la => classIds.Contains(la.ClassId))
            .Select(la => new DeadlineDto
            {
                Id = la.Id,
                Title = la.Lesson.Title,
                Teacher = la.Teacher.FullName,
                Duedate = (la.DueDate ?? la.AssignedAt.AddDays(7)).ToString("dd/MM/yyyy"),
                Status = _db.LessonProgresses
                    .Any(p => p.UserId == userId && p.LessonId == la.LessonId && p.Status == LessonStatus.Completed) 
                    ? "Completed" : "Pending"
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            AverageAccuracy = Math.Round(avgAcc, 1),
            CurrentStreak = streak?.CurrentStreak ?? 0,
            SuggestedLesson = lessonDto,
            Deadlines = assignments
        };
    }

    public async Task<ProgressStatsDto> GetProgressStatsAsync(Guid userId)
    {
        // Compute average directly on DB
        double avgAcc = await _db.PracticeAttempts
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;
        avgAcc *= 100;

        var completedLessons = await _db.LessonProgresses
            .CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed);
        var masteredSigns = await _db.SignProgresses
            .CountAsync(p => p.UserId == userId && p.IsMastered);

        return new ProgressStatsDto
        {
            OverallAccuracy = Math.Round(avgAcc, 1),
            TotalLessonsCompleted = completedLessons,
            TotalSignsMastered = masteredSigns
        };
    }
}
