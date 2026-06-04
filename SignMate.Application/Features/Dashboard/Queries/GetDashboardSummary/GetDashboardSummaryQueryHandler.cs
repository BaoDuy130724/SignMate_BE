using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.DTOs.Dashboard;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Handler tổng hợp dữ liệu trang chủ học viên. Mọi phép tính (trung bình điểm, đếm bài hoàn thành)
/// được đẩy xuống DB để tránh nạp toàn bộ bản ghi vào bộ nhớ.
/// </summary>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var userId = query.UserId;

        var streak = await _unitOfWork.Repository<Streak>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        // Độ chính xác trung bình: tính ngay trên DB thay vì load toàn bộ attempt.
        double avgAccuracy = (await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync(cancellationToken) ?? 0) * 100;

        // Bài học gợi ý: lấy bài đã publish đầu tiên (logic gợi ý cơ bản hiện tại).
        var suggested = await _unitOfWork.Repository<Lesson>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsPublished, cancellationToken);

        LessonDto? suggestedLesson = suggested is null ? null : new LessonDto
        {
            Id = suggested.Id,
            CourseId = suggested.CourseId,
            Title = suggested.Title,
            Description = suggested.Description,
            DurationSeconds = suggested.DurationSeconds,
            Topic = suggested.Topic
        };

        // Deadline: bài tập được giao cho các lớp mà học viên đang tham gia.
        var classIds = await _unitOfWork.Repository<ClassStudent>().Query()
            .Where(cs => cs.StudentId == userId)
            .Select(cs => cs.ClassId)
            .ToListAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

        // Tập bài đã hoàn thành để gắn trạng thái cho từng deadline.
        var completedLessonIds = await _unitOfWork.Repository<LessonProgress>().Query()
            .Where(p => p.UserId == userId && p.Status == LessonStatus.Completed)
            .Select(p => p.LessonId)
            .ToListAsync(cancellationToken);

        var deadlines = rawAssignments.Select(la => new DeadlineDto
        {
            Id = la.Id,
            Title = la.LessonTitle,
            Teacher = la.TeacherName,
            Duedate = la.DueDate.ToString("dd/MM/yyyy"),
            Status = completedLessonIds.Contains(la.LessonId) ? "Completed" : "Pending"
        }).ToList();

        return new DashboardSummaryDto
        {
            AverageAccuracy = Math.Round(avgAccuracy, 1),
            CurrentStreak = streak?.CurrentStreak ?? 0,
            SuggestedLesson = suggestedLesson,
            Deadlines = deadlines
        };
    }
}
