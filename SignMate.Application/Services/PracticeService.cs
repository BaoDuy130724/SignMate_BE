using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class PracticeService : IPracticeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobService _blobService;
    private readonly IAIClientService _aiClient;
    private readonly IStreakService _streakService;
    private readonly IGeminiService _geminiService;

    public PracticeService(IUnitOfWork unitOfWork, IBlobService blobService,
        IAIClientService aiClient, IStreakService streakService, IGeminiService geminiService)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _aiClient = aiClient;
        _streakService = streakService;
        _geminiService = geminiService;
    }

    public async Task<StartSessionResponse> StartSessionAsync(int userId, StartSessionRequest request)
    {
        _ = await _unitOfWork.Repository<Sign>().GetByIdAsync(request.SignId) 
            ?? throw new ArgumentException("Sign not found.");

        var session = new PracticeSession
        {
            Id = 0, UserId = userId,
            SignId = request.SignId, StartedAt = DateTime.UtcNow, TotalAttempts = 0
        };

        await _unitOfWork.Repository<PracticeSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return new StartSessionResponse { SessionId = session.Id };
    }

    public async Task<AttemptResponse> SubmitAttemptAsync(int userId, int sessionId, Stream videoStream, string fileName)
    {
        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        if (session.EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended.");

        var videoUrl = await _blobService.UploadAsync(videoStream, fileName, "video/mp4");

        var aiResult = await _aiClient.AnalyzeAsync(videoUrl, session.SignId.ToString(), session.Sign.ReferenceKeypointData);

        var attempt = new PracticeAttempt
        {
            Id = 0, SessionId = sessionId,
            VideoClipUrl = videoUrl, RecordedAt = DateTime.UtcNow,
            OverallScore = aiResult.OverallScore
        };
        await _unitOfWork.Repository<PracticeAttempt>().AddAsync(attempt);

        foreach (var fb in aiResult.Feedbacks)
        {
            if (!Enum.TryParse<FeedbackType>(fb.Type, true, out var fbType)) continue;
            await _unitOfWork.Repository<AIFeedback>().AddAsync(new AIFeedback
            {
                Id = 0, AttemptId = attempt.Id,
                FeedbackType = fbType, Score = fb.Score,
                Message = fb.Message, KeypointData = aiResult.KeypointData,
                CreatedAt = DateTime.UtcNow
            });
        }

        session.TotalAttempts++;

        var signProgress = await _unitOfWork.Repository<SignProgress>().Query()
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == session.SignId);

        if (signProgress == null)
        {
            signProgress = new SignProgress
            {
                Id = 0, UserId = userId, SignId = session.SignId,
                AttemptCount = 1, IsMastered = aiResult.OverallScore >= 0.8f,
                LastPracticedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<SignProgress>().AddAsync(signProgress);
        }
        else
        {
            signProgress.AttemptCount++;
            signProgress.LastPracticedAt = DateTime.UtcNow;
            if (aiResult.OverallScore >= 0.8f) signProgress.IsMastered = true;
        }

        await _streakService.RecordActivityAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return new AttemptResponse
        {
            AttemptId = attempt.Id,
            OverallScore = aiResult.OverallScore,
            Feedbacks = aiResult.Feedbacks.Select(f => new FeedbackDto
            {
                Type = f.Type, Score = f.Score, Message = f.Message
            }).ToList()
        };
    }

    public async Task EndSessionAsync(int userId, int sessionId)
    {
        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        session.EndedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<PracticeHistoryDto>> GetHistoryAsync(int userId, int signId)
    {
        return await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(s => s.UserId == userId && s.SignId == signId)
            .OrderByDescending(s => s.StartedAt)
            .Select(s => new PracticeHistoryDto
            {
                SessionId = s.Id, StartedAt = s.StartedAt,
                EndedAt = s.EndedAt, TotalAttempts = s.TotalAttempts,
                Attempts = s.Attempts.OrderByDescending(a => a.RecordedAt).Select(a => new AttemptSummaryDto
                {
                    Id = a.Id, RecordedAt = a.RecordedAt, OverallScore = a.OverallScore,
                    Feedbacks = a.Feedbacks.Select(f => new FeedbackDto
                    {
                        Type = f.FeedbackType.ToString(), Score = f.Score, Message = f.Message
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<AttemptResponse> ReportResultAsync(int userId, ReportResultRequest request)
    {
        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        if (session.EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended.");

        var attempt = new PracticeAttempt
        {
            Id = 0, SessionId = request.SessionId,
            VideoClipUrl = string.Empty, // No video uploaded in this flow
            RecordedAt = DateTime.UtcNow,
            OverallScore = request.OverallScore
        };
        await _unitOfWork.Repository<PracticeAttempt>().AddAsync(attempt);

        foreach (var fb in request.Feedbacks)
        {
            if (!Enum.TryParse<FeedbackType>(fb.Type, true, out var fbType)) continue;
            await _unitOfWork.Repository<AIFeedback>().AddAsync(new AIFeedback
            {
                Id = 0, AttemptId = attempt.Id,
                FeedbackType = fbType, Score = fb.Score,
                Message = fb.Message, KeypointData = null,
                CreatedAt = DateTime.UtcNow
            });
        }

        session.TotalAttempts++;

        var signProgress = await _unitOfWork.Repository<SignProgress>().Query()
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == session.SignId);

        if (signProgress == null)
        {
            signProgress = new SignProgress
            {
                Id = 0, UserId = userId, SignId = session.SignId,
                AttemptCount = 1, IsMastered = request.OverallScore >= 0.8f,
                LastPracticedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<SignProgress>().AddAsync(signProgress);
        }
        else
        {
            signProgress.AttemptCount++;
            signProgress.LastPracticedAt = DateTime.UtcNow;
            if (request.OverallScore >= 0.8f) signProgress.IsMastered = true;
        }

        await _streakService.RecordActivityAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        // --- Gemini AI Feedback (Pro plan only) ---
        string? geminiFeedback = null;
        var activeSub = await _unitOfWork.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow);

        if (activeSub?.Plan.Type == PlanType.Pro)
        {
            var previousAttempt = await _unitOfWork.Repository<PracticeAttempt>().Query()
                .Where(a => a.SessionId == request.SessionId && a.Id != attempt.Id)
                .OrderByDescending(a => a.RecordedAt)
                .FirstOrDefaultAsync();

            var geminiContext = new GeminiFeedbackContext
            {
                SignWord = session.Sign.Word,
                OverallScore = request.OverallScore,
                DtwFeedbacks = request.Feedbacks.Select(f => new FeedbackItem
                {
                    Type = f.Type, Score = f.Score, Message = f.Message
                }).ToList(),
                AttemptNumber = session.TotalAttempts,
                PreviousScore = previousAttempt?.OverallScore
            };

            geminiFeedback = await _geminiService.GenerateDetailedFeedbackAsync(geminiContext);
        }

        return new AttemptResponse
        {
            AttemptId = attempt.Id,
            OverallScore = request.OverallScore,
            Feedbacks = request.Feedbacks,
            GeminiFeedback = geminiFeedback
        };
    }
}
