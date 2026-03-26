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
        var streak = await _db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        
        var attempts = await _db.PracticeSessions
            .Where(s => s.UserId == userId)
            .SelectMany(s => s.Attempts)
            .ToListAsync();
            
        double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;

        // Suggested Lesson logic (mock simple for now)
        var suggested = await _db.Lessons.FirstOrDefaultAsync(l => l.IsPublished);
        LessonDto? lessonDto = suggested == null ? null : new LessonDto
        {
            Id = suggested.Id, CourseId = suggested.CourseId, Title = suggested.Title,
            Description = suggested.Description, DurationSeconds = suggested.DurationSeconds,
            Topic = suggested.Topic
        };

        return new DashboardSummaryDto
        {
            AverageAccuracy = Math.Round(avgAcc, 1),
            CurrentStreak = streak?.CurrentStreak ?? 0,
            SuggestedLesson = lessonDto
        };
    }

    public async Task<ProgressStatsDto> GetProgressStatsAsync(Guid userId)
    {
        var attempts = await _db.PracticeSessions.Where(s => s.UserId == userId).SelectMany(s => s.Attempts).ToListAsync();
        double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;
        
        var completedLessons = await _db.LessonProgresses.CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed);
        var masteredSigns = await _db.SignProgresses.CountAsync(p => p.UserId == userId && p.IsMastered);

        return new ProgressStatsDto
        {
            OverallAccuracy = Math.Round(avgAcc, 1),
            TotalLessonsCompleted = completedLessons,
            TotalSignsMastered = masteredSigns
        };
    }
}
