using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Features.Subscription.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Queries.GetAttemptFeedback;

/// <summary>
/// Handler cho <see cref="GetAttemptFeedbackQuery"/>: sinh nhận xét chi tiết Gemini cho một lượt thử
/// đã chấm. Áp ĐÚNG cổng phân tầng như SubmitAttempt cũ — chỉ gói Pro/B2B mới có (UC-06); không thuộc
/// gói trả về null (không lỗi). Số lần gọi Gemini không tăng: app gọi đúng 1 lần/lượt sau khi thấy điểm.
/// </summary>
public class GetAttemptFeedbackQueryHandler
    : IRequestHandler<GetAttemptFeedbackQuery, AttemptFeedbackResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeminiService _geminiService;

    public GetAttemptFeedbackQueryHandler(IUnitOfWork unitOfWork, IGeminiService geminiService)
    {
        _unitOfWork = unitOfWork;
        _geminiService = geminiService;
    }

    /// <inheritdoc />
    public async Task<AttemptFeedbackResponse> Handle(
        GetAttemptFeedbackQuery query, CancellationToken cancellationToken)
    {
        // Tải lượt thử kèm phiên/ký hiệu/điểm chi tiết; lọc theo UserId để chống xem lượt của người khác.
        var attempt = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .AsNoTracking()
            .Include(a => a.Session).ThenInclude(s => s.Sign)
            .Include(a => a.Feedbacks)
            .FirstOrDefaultAsync(
                a => a.Id == query.AttemptId && a.Session.UserId == query.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PracticeAttempt), query.AttemptId);

        // Cổng quyền: chỉ Pro/B2B mới có nhận xét chi tiết (giống TryGenerateGeminiFeedbackAsync cũ).
        var activeSub = await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(
                s => s.UserId == query.UserId && s.IsActive && s.EndDate > DateTime.UtcNow,
                cancellationToken);

        if (activeSub is null || !PlanEntitlements.HasProFeatures(activeSub.Plan.Type))
            return new AttemptFeedbackResponse { Feedback = null };

        // Lượt thử liền trước trong cùng phiên (để Gemini so tiến bộ) + thứ tự lượt hiện tại.
        var previousScore = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .AsNoTracking()
            .Where(a => a.SessionId == attempt.SessionId && a.RecordedAt < attempt.RecordedAt)
            .OrderByDescending(a => a.RecordedAt)
            .Select(a => (float?)a.OverallScore)
            .FirstOrDefaultAsync(cancellationToken);

        var attemptNumber = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .AsNoTracking()
            .CountAsync(
                a => a.SessionId == attempt.SessionId && a.RecordedAt <= attempt.RecordedAt,
                cancellationToken);

        var context = new GeminiFeedbackContext
        {
            SignWord = attempt.Session.Sign.Word,
            OverallScore = attempt.OverallScore,
            DtwFeedbacks = attempt.Feedbacks
                .Select(f => new FeedbackItem
                {
                    Type = f.FeedbackType.ToString(),
                    Score = f.Score,
                    Message = f.Message
                })
                .ToList(),
            AttemptNumber = attemptNumber,
            PreviousScore = previousScore
        };

        var feedback = await _geminiService.GenerateDetailedFeedbackAsync(context);
        return new AttemptFeedbackResponse { Feedback = feedback };
    }
}
