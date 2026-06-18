using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
/// Nhận xét chi tiết Gemini KHÔNG sinh ở đây nữa (trước đây await chặn tới 10s) — điểm trả về ngay,
/// app lấy nhận xét qua <c>GET /api/practice/attempt/{id}/feedback</c> (GetAttemptFeedbackQuery).
/// </summary>
public class SubmitAttemptCommandHandler : IRequestHandler<SubmitAttemptCommand, AttemptResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobService _blobService;
    private readonly IAIClientService _aiClient;
    private readonly ILogger<SubmitAttemptCommandHandler> _logger;

    public SubmitAttemptCommandHandler(
        IUnitOfWork unitOfWork, IBlobService blobService, IAIClientService aiClient,
        ILogger<SubmitAttemptCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _aiClient = aiClient;
        _logger = logger;
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
        // Gửi kèm video mẫu + tên ký hiệu để AI chạy giám khảo Gemini multimodal (chấm THẬT).
        var aiResult = await _aiClient.AnalyzeAsync(
            videoUrl, session.SignId.ToString(), session.Sign.ReferenceKeypointData,
            referenceVideoUrl: session.Sign.VideoUrl, signWord: session.Sign.Word);

        // "Đạt/Chưa đạt": ưu tiên verdict của giám khảo; nếu giám khảo không chạy được (Gemini
        // lỗi / không bật) thì fallback ngưỡng DTW 0.7 như cũ để không chặn người dùng.
        var judge = aiResult.Judge;
        var isCorrect = judge != null
            ? string.Equals(judge.Verdict, "dat", StringComparison.OrdinalIgnoreCase)
            : aiResult.OverallScore >= 0.7f;

        // DEBUG (Information — hiện ở Development, tắt ở Production vì log level = Warning):
        // soi đầu vào/ra của bước chấm để dò lỗi business logic verdict→IsCorrect→tiering.
        if (judge != null)
        {
            _logger.LogInformation(
                "[SubmitAttempt] user={UserId} sign={SignId} '{Word}' | DTW={Dtw:F3} | JUDGE verdict={Verdict} conf={Conf:F2} | hand={Hand} loc={Loc} move={Move} palm={Palm} | summary=\"{Summary}\"",
                command.UserId, session.SignId, session.Sign.Word, aiResult.OverallScore,
                judge.Verdict, judge.Confidence,
                judge.HandShape.Status, judge.Location.Status, judge.Movement.Status, judge.PalmOrientation.Status,
                judge.Summary);
        }
        else
        {
            _logger.LogWarning(
                "[SubmitAttempt] user={UserId} sign={SignId} '{Word}' | DTW={Dtw:F3} | JUDGE=none (Gemini lỗi/không bật → fallback ngưỡng DTW 0.7)",
                command.UserId, session.SignId, session.Sign.Word, aiResult.OverallScore);
        }
        _logger.LogInformation(
            "[SubmitAttempt] => IsCorrect={IsCorrect} (nguồn={Source})",
            isCorrect, judge != null ? "verdict giám khảo" : "DTW>=0.7");

        var attempt = new PracticeAttempt
        {
            SessionId = session.Id,
            VideoClipUrl = videoUrl,
            RecordedAt = DateTime.UtcNow,
            OverallScore = aiResult.OverallScore,
            JudgeVerdict = judge?.Verdict,
            JudgeRubricJson = judge != null ? JsonSerializer.Serialize(judge) : null,
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

        // Gói cước để trim rubric theo tầng (Free 1 câu / Basic tiêu chí yếu / Pro-B2B đầy đủ).
        var planType = await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.UserId == command.UserId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .Select(s => (PlanType?)s.Plan.Type)
            .FirstOrDefaultAsync(cancellationToken) ?? PlanType.Free;

        var rubric = judge != null ? JudgeTiering.BuildTiered(judge, planType) : null;
        // DEBUG: phân tầng theo gói — số tiêu chí trả về cho client (Free=0/null, Basic=tiêu chí yếu, Pro/B2B=4).
        _logger.LogInformation(
            "[SubmitAttempt] attempt={AttemptId} saved | plan={Plan} | rubricCriteriaTrả={Count}",
            attempt.Id, planType, rubric?.Criteria.Count ?? 0);

        return new AttemptResponse
        {
            AttemptId = attempt.Id,
            OverallScore = aiResult.OverallScore,
            Feedbacks = aiResult.Feedbacks
                .Select(f => new FeedbackDto { Type = f.Type, Score = f.Score, Message = f.Message })
                .ToList(),
            GeminiFeedback = null,
            // Đánh giá thật, phân tầng theo gói. Summary (1 câu) cho mọi gói; rubric chi tiết
            // trim theo gói. IsCorrect là nguồn sự thật mới cho UI (verdict giám khảo / fallback DTW).
            IsCorrect = isCorrect,
            Verdict = judge?.Verdict,
            Summary = judge?.Summary,
            Rubric = rubric
        };
    }
}
