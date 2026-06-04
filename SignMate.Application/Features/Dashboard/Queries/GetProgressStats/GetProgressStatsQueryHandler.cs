using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Dashboard;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Dashboard.Queries.GetProgressStats;

/// <summary>
/// Handler tính các chỉ số tiến độ học tập. Điểm theo chủ đề được gom nhóm phía ứng dụng
/// sau khi lấy về cặp (chủ đề, điểm) để dễ xử lý ngưỡng "chủ đề yếu" (&lt; 70%).
/// </summary>
public class GetProgressStatsQueryHandler : IRequestHandler<GetProgressStatsQuery, ProgressStatsDto>
{
    private const int WeakTopicThreshold = 70;

    private readonly IUnitOfWork _unitOfWork;

    public GetProgressStatsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<ProgressStatsDto> Handle(GetProgressStatsQuery query, CancellationToken cancellationToken)
    {
        var userId = query.UserId;

        double avgAccuracy = (await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync(cancellationToken) ?? 0) * 100;

        var completedLessons = await _unitOfWork.Repository<LessonProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed, cancellationToken);

        var masteredSigns = await _unitOfWork.Repository<SignProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.IsMastered, cancellationToken);

        // Lấy cặp (chủ đề, điểm) rồi gom nhóm tính trung bình theo chủ đề.
        var topicScores = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => new
            {
                Topic = a.Session.Sign.Lesson.Topic ?? "Chung",
                Score = a.OverallScore
            })
            .ToListAsync(cancellationToken);

        var accuracyByTopic = topicScores
            .GroupBy(x => x.Topic)
            .ToDictionary(
                g => g.Key,
                g => (int)Math.Round(g.Average(x => x.Score) * 100));

        var weakTopics = accuracyByTopic
            .Where(kv => kv.Value < WeakTopicThreshold)
            .Select(kv => kv.Key)
            .ToList();

        return new ProgressStatsDto
        {
            OverallAccuracy = Math.Round(avgAccuracy, 1),
            TotalLessonsCompleted = completedLessons,
            TotalSignsMastered = masteredSigns,
            WeakTopics = weakTopics,
            AccuracyByTopic = accuracyByTopic
        };
    }
}
