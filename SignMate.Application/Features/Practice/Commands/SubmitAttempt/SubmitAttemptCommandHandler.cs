using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Features.Practice.Common;
using SignMate.Application.Features.Streaks.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Commands.SubmitAttempt;

/// <summary>
/// Handler cho <see cref="SubmitAttemptCommand"/>: tải video lên blob, gọi AI chấm điểm rồi ghi lượt thử
/// kèm phản hồi, cập nhật tiến độ ký hiệu và streak. Các thao tác bên ngoài (blob, AI) thực hiện trước
/// transaction; toàn bộ ghi DB phụ thuộc nhau gói trong một transaction để đảm bảo Atomicity.
/// </summary>
public class SubmitAttemptCommandHandler : IRequestHandler<SubmitAttemptCommand, AttemptResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobService _blobService;
    private readonly IAIClientService _aiClient;
    private readonly IGeminiService _geminiService;

    public SubmitAttemptCommandHandler(
        IUnitOfWork unitOfWork, IBlobService blobService, IAIClientService aiClient, IGeminiService geminiService)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _aiClient = aiClient;
        _geminiService = geminiService;
    }

    /// <inheritdoc />
    public async Task<AttemptResponse> Handle(SubmitAttemptCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && s.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PracticeSession), command.SessionId);

        if (session.EndedAt.HasValue)
            throw new ConflictException("Phiên luyện tập đã kết thúc.");

        // Thao tác ngoài (blob + AI) đặt trước transaction: nếu lỗi thì chưa ghi DB.
        var videoUrl = await _blobService.UploadAsync(command.VideoStream, command.FileName, "video/mp4");
        var aiResult = await _aiClient.AnalyzeAsync(
            videoUrl, session.SignId.ToString(), session.Sign.ReferenceKeypointData);

        var attempt = new PracticeAttempt
        {
            SessionId = session.Id,
            VideoClipUrl = videoUrl,
            RecordedAt = DateTime.UtcNow,
            OverallScore = aiResult.OverallScore,
            Feedbacks = aiResult.Feedbacks
                .Where(fb => Enum.TryParse<FeedbackType>(fb.Type, true, out _))
                .Select(fb => new AIFeedback
                {
                    FeedbackType = Enum.Parse<FeedbackType>(fb.Type, true),
                    Score = fb.Score,
                    Message = fb.Message,
                    KeypointData = aiResult.KeypointData,
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
                _unitOfWork, command.UserId, session.SignId, aiResult.OverallScore, cancellationToken);
            await StreakActivity.RecordAsync(_unitOfWork, command.UserId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // Sau commit: nếu là gói Pro thì sinh phản hồi chi tiết bằng Gemini (thao tác ngoài transaction).
        var geminiFeedback = await TryGenerateGeminiFeedbackAsync(
            command.UserId, session, aiResult, attempt.Id, cancellationToken);

        return new AttemptResponse
        {
            AttemptId = attempt.Id,
            OverallScore = aiResult.OverallScore,
            Feedbacks = aiResult.Feedbacks
                .Select(f => new FeedbackDto { Type = f.Type, Score = f.Score, Message = f.Message })
                .ToList(),
            GeminiFeedback = geminiFeedback
        };
    }

    /// <summary>
    /// Sinh phản hồi chi tiết bằng Gemini cho người dùng gói Pro (đối chiếu điểm lượt thử liền trước).
    /// Trả về null nếu người dùng không thuộc gói Pro — đúng phân tầng UC-06 (Pro → detailed).
    /// </summary>
    private async Task<string?> TryGenerateGeminiFeedbackAsync(
        int userId, PracticeSession session, AIAnalysisResult aiResult,
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
            OverallScore = aiResult.OverallScore,
            DtwFeedbacks = aiResult.Feedbacks,
            AttemptNumber = session.TotalAttempts,
            PreviousScore = previousAttempt?.OverallScore
        };

        return await _geminiService.GenerateDetailedFeedbackAsync(context);
    }
}
