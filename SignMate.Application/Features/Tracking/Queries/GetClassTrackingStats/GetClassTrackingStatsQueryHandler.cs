using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.StudentTracking;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Tracking.Queries.GetClassTrackingStats;

/// <summary>
/// Handler tính độ chính xác trung bình và tần suất luyện tập cho từng học viên trong lớp.
/// Gom toàn bộ điểm các lượt luyện tập theo học viên trong một truy vấn rồi tổng hợp phía ứng dụng,
/// tránh truy vấn lặp theo từng học viên (N+1).
/// </summary>
public class GetClassTrackingStatsQueryHandler
    : IRequestHandler<GetClassTrackingStatsQuery, List<StudentTrackingStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClassTrackingStatsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<StudentTrackingStatsDto>> Handle(
        GetClassTrackingStatsQuery query, CancellationToken cancellationToken)
    {
        var students = await _unitOfWork.Repository<ClassStudent>().Query()
            .AsNoTracking()
            .Where(cs => cs.ClassId == query.ClassId)
            .Select(cs => new { cs.StudentId, cs.Student.FullName })
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.StudentId).ToList();

        // Lấy điểm từng lượt luyện t.gắn với học viên trong lớp (một truy vấn duy nhất).
        var attempts = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.Session.UserId))
            .Select(a => new { a.Session.UserId, a.OverallScore })
            .ToListAsync(cancellationToken);

        var statsByStudent = attempts
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => new { Avg = g.Average(x => x.OverallScore), Count = g.Count() });

        return students.Select(s =>
        {
            statsByStudent.TryGetValue(s.StudentId, out var stat);
            return new StudentTrackingStatsDto
            {
                StudentId = s.StudentId,
                FullName = s.FullName,
                AccuracyPercent = stat is null ? 0 : Math.Round(stat.Avg * 100, 1),
                PracticeFrequencyDays = stat?.Count ?? 0
            };
        }).ToList();
    }
}
