using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Dashboard;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId)
    {
        var streak = await _unitOfWork.Repository<Streak>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        // Compute average directly on DB instead of loading all attempts into memory
        double avgAcc = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;
        avgAcc *= 100;

        // Suggested Lesson logic
        var suggested = await _unitOfWork.Repository<Lesson>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsPublished);

        LessonDto? lessonDto = suggested == null ? null : new LessonDto
        {
            Id = suggested.Id, CourseId = suggested.CourseId, Title = suggested.Title,
            Description = suggested.Description, DurationSeconds = suggested.DurationSeconds,
            Topic = suggested.Topic
        };

        // Fetch Student's classes to get assignments
        var classIds = await _unitOfWork.Repository<ClassStudent>().Query()
            .Where(cs => cs.StudentId == userId)
            .Select(cs => cs.ClassId)
            .ToListAsync();

        var rawAssignments = await _unitOfWork.Repository<LessonAssignment>().Query()
            .AsNoTracking()
            .Where(la => classIds.Contains(la.ClassId))
            .Select(la => new
            {
                la.Id,
                LessonTitle = la.Lesson.Title,
                TeacherName = la.Teacher.FullName,
                DueDate = la.DueDate ?? la.AssignedAt.AddDays(7),
                la.LessonId
            })
            .ToListAsync();

        // Fetch completed lesson IDs for this user to determine status
        var completedLessonIds = await _unitOfWork.Repository<LessonProgress>().Query()
            .Where(p => p.UserId == userId && p.Status == LessonStatus.Completed)
            .Select(p => p.LessonId)
            .ToListAsync();

        var assignments = rawAssignments.Select(la => new DeadlineDto
        {
            Id = la.Id,
            Title = la.LessonTitle,
            Teacher = la.TeacherName,
            Duedate = la.DueDate.ToString("dd/MM/yyyy"),
            Status = completedLessonIds.Contains(la.LessonId) ? "Completed" : "Pending"
        }).ToList();

        return new DashboardSummaryDto
        {
            AverageAccuracy = Math.Round(avgAcc, 1),
            CurrentStreak = streak?.CurrentStreak ?? 0,
            SuggestedLesson = lessonDto,
            Deadlines = assignments
        };
    }

    public async Task<ProgressStatsDto> GetProgressStatsAsync(int userId)
    {
        // Compute average directly on DB
        double avgAcc = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;
        avgAcc *= 100;

        var completedLessons = await _unitOfWork.Repository<LessonProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed);
        var masteredSigns = await _unitOfWork.Repository<SignProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.IsMastered);

        // Accuracy by topic: group practice attempts by lesson topic
        var topicScores = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => new
            {
                Topic = a.Session.Sign.Lesson.Topic ?? "Chung",
                Score = a.OverallScore
            })
            .ToListAsync();

        var accuracyByTopic = topicScores
            .GroupBy(x => x.Topic)
            .ToDictionary(
                g => g.Key,
                g => (int)Math.Round(g.Average(x => x.Score) * 100)
            );

        // Weak topics: topics where accuracy < 70%
        var weakTopics = accuracyByTopic
            .Where(kv => kv.Value < 70)
            .Select(kv => kv.Key)
            .ToList();

        return new ProgressStatsDto
        {
            OverallAccuracy = Math.Round(avgAcc, 1),
            TotalLessonsCompleted = completedLessons,
            TotalSignsMastered = masteredSigns,
            WeakTopics = weakTopics,
            AccuracyByTopic = accuracyByTopic
        };
    }
}
