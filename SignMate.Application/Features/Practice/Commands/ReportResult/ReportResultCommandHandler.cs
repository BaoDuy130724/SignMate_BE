using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Features.Practice.Common;
using SignMate.Application.Features.Streaks.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Commands.ReportResult;

/// <summary>
/// Handler cho <see cref="ReportResultCommand"/>: ghi lượt thử đã chấm sẵn (không có video) kèm phản hồi,
/// cập nhật tiến độ ký hiệu và streak trong một transaction. Sau khi commit, nếu người dùng đang dùng gói
/// Pro thì gọi Gemini sinh phản hồi chi tiết (thao tác ngoài, không nằm trong transaction ghi).
/// </summary>
public class ReportResultCommandHandler : IRequestHandler<ReportResultCommand, AttemptResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeminiService _geminiService;

    public ReportResultCommandHandler(IUnitOfWork unitOfWork, IGeminiService geminiService)
    {
        _unitOfWork = unitOfWork;
        _geminiService = geminiService;
    }

    /// <inheritdoc />
    public async Task<AttemptResponse> Handle(ReportResultCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PracticeSession), request.SessionId);

        if (session.EndedAt.HasValue)
            throw new ConflictException("Phiên luyện tập đã kết thúc.");

        var attempt = new PracticeAttempt
        {
            SessionId = session.Id,
            VideoClipUrl = string.Empty, // Luồng này không tải video lên.
            RecordedAt = DateTime.UtcNow,
            OverallScore = request.OverallScore,
            Feedbacks = request.Feedbacks
                .Where(fb => Enum.TryParse<FeedbackType>(fb.Type, true, out _))
                .Select(fb => new AIFeedback
                {
                    FeedbackType = Enum.Parse<FeedbackType>(fb.Type, true),
                    Score = fb.Score,
                    Message = fb.Message,
                    KeypointData = null,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList()
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            await _unitOfWork.Repository<PracticeAttempt>().AddAsync(attempt);
            session.TotalAttempts++;

            await SignProgressUpdater.UpsertAsync(
                _unitOfWork, command.UserId, session.SignId, request.OverallScore, cancellationToken);
            await StreakActivity.RecordAsync(_unitOfWork, command.UserId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var geminiFeedback = await TryGenerateGeminiFeedbackAsync(
            command.UserId, session, request, attempt.Id, cancellationToken);

        return new AttemptResponse
        {
            AttemptId = attempt.Id,
            OverallScore = request.OverallScore,
            Feedbacks = request.Feedbacks,
            GeminiFeedback = geminiFeedback
        };
    }

    /// <summary>
    /// Sinh phản hồi chi tiết bằng Gemini cho người dùng gói Pro, có đối chiếu điểm lượt thử liền trước.
    /// Trả về null nếu người dùng không thuộc gói Pro.
    /// </summary>
    private async Task<string?> TryGenerateGeminiFeedbackAsync(
        int userId, PracticeSession session, ReportResultRequest request,
        int currentAttemptId, CancellationToken cancellationToken)
    {
        var activeSub = await _unitOfWork.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow, cancellationToken);

        if (activeSub?.Plan.Type != PlanType.Pro)
            return null;

        var previousAttempt = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.SessionId == session.Id && a.Id != currentAttemptId)
            .OrderByDescending(a => a.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var context = new GeminiFeedbackContext
        {
            SignWord = session.Sign.Word,
            OverallScore = request.OverallScore,
            DtwFeedbacks = request.Feedbacks
                .Select(f => new FeedbackItem { Type = f.Type, Score = f.Score, Message = f.Message })
                .ToList(),
            AttemptNumber = session.TotalAttempts,
            PreviousScore = previousAttempt?.OverallScore
        };

        return await _geminiService.GenerateDetailedFeedbackAsync(context);
    }
}
