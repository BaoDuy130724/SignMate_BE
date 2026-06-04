using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Queries.GetPracticeHistory;

/// <summary>
/// Handler cho <see cref="GetPracticeHistoryQuery"/>: projection các phiên luyện tập của người dùng cho
/// một ký hiệu, kèm chi tiết từng lượt thử và phản hồi, sắp xếp mới nhất trước. Read-side, AsNoTracking.
/// </summary>
public class GetPracticeHistoryQueryHandler : IRequestHandler<GetPracticeHistoryQuery, List<PracticeHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPracticeHistoryQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<PracticeHistoryDto>> Handle(GetPracticeHistoryQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<PracticeSession>().Query()
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId && s.SignId == query.SignId)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new PracticeHistoryDto
            {
                SessionId = s.Id,
                StartedAt = s.StartedAt,
                EndedAt = s.EndedAt,
                TotalAttempts = s.TotalAttempts,
                Attempts = s.Attempts.OrderByDescending(a => a.RecordedAt).Select(a => new AttemptSummaryDto
                {
                    Id = a.Id,
                    RecordedAt = a.RecordedAt,
                    OverallScore = a.OverallScore,
                    Feedbacks = a.Feedbacks.Select(f => new FeedbackDto
                    {
                        Type = f.FeedbackType.ToString(),
                        Score = f.Score,
                        Message = f.Message
                    }).ToList()
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
