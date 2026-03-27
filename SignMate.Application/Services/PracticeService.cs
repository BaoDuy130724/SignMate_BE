using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class PracticeService : IPracticeService
{
    private readonly ISignMateDbContext _db;
    private readonly IBlobService _blobService;
    private readonly IAIClientService _aiClient;
    private readonly IStreakService _streakService;
    private readonly IGeminiService _geminiService;

    public PracticeService(ISignMateDbContext db, IBlobService blobService,
        IAIClientService aiClient, IStreakService streakService, IGeminiService geminiService)
    {
        _db = db;
        _blobService = blobService;
        _aiClient = aiClient;
        _streakService = streakService;
        _geminiService = geminiService;
    }

    public async Task<StartSessionResponse> StartSessionAsync(Guid userId, StartSessionRequest request)
    {
        _ = await _db.Signs.FindAsync(request.SignId) ?? throw new ArgumentException("Sign not found.");

        var session = new PracticeSession
        {
            Id = Guid.NewGuid(), UserId = userId,
            SignId = request.SignId, StartedAt = DateTime.UtcNow, TotalAttempts = 0
        };

        _db.PracticeSessions.Add(session);
        await _db.SaveChangesAsync();
        return new StartSessionResponse { SessionId = session.Id };
    }

    public async Task<AttemptResponse> SubmitAttemptAsync(Guid userId, Guid sessionId, Stream videoStream, string fileName)
    {
        var session = await _db.PracticeSessions.Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        if (session.EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended.");

        var videoUrl = await _blobService.UploadAsync(videoStream, fileName, "video/mp4");

        var aiResult = await _aiClient.AnalyzeAsync(videoUrl, session.SignId.ToString(), session.Sign.ReferenceKeypointData);

        var attempt = new PracticeAttempt
        {
            Id = Guid.NewGuid(), SessionId = sessionId,
            VideoClipUrl = videoUrl, RecordedAt = DateTime.UtcNow,
            OverallScore = aiResult.OverallScore
        };
        _db.PracticeAttempts.Add(attempt);

        foreach (var fb in aiResult.Feedbacks)
        {
            if (!Enum.TryParse<FeedbackType>(fb.Type, true, out var fbType)) continue;
            _db.AIFeedbacks.Add(new AIFeedback
            {
                Id = Guid.NewGuid(), AttemptId = attempt.Id,
                FeedbackType = fbType, Score = fb.Score,
                Message = fb.Message, KeypointData = aiResult.KeypointData,
                CreatedAt = DateTime.UtcNow
            });
        }

        session.TotalAttempts++;

        var signProgress = await _db.SignProgresses
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == session.SignId);

        if (signProgress == null)
        {
            signProgress = new SignProgress
            {
                Id = Guid.NewGuid(), UserId = userId, SignId = session.SignId,
                AttemptCount = 1, IsMastered = aiResult.OverallScore >= 0.8f,
                LastPracticedAt = DateTime.UtcNow
            };
            _db.SignProgresses.Add(signProgress);
        }
        else
        {
            signProgress.AttemptCount++;
            signProgress.LastPracticedAt = DateTime.UtcNow;
            if (aiResult.OverallScore >= 0.8f) signProgress.IsMastered = true;
        }

        await _streakService.RecordActivityAsync(userId);
        await _db.SaveChangesAsync();

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

    public async Task EndSessionAsync(Guid userId, Guid sessionId)
    {
        var session = await _db.PracticeSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        session.EndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<PracticeHistoryDto>> GetHistoryAsync(Guid userId, Guid signId)
    {
        return await _db.PracticeSessions
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

    public async Task<AttemptResponse> ReportResultAsync(Guid userId, ReportResultRequest request)
    {
        var session = await _db.PracticeSessions.Include(s => s.Sign)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId)
            ?? throw new ArgumentException("Session not found.");

        if (session.EndedAt.HasValue)
            throw new InvalidOperationException("Session already ended.");

        var attempt = new PracticeAttempt
        {
            Id = Guid.NewGuid(), SessionId = request.SessionId,
            VideoClipUrl = string.Empty, // No video uploaded in this flow
            RecordedAt = DateTime.UtcNow,
            OverallScore = request.OverallScore
        };
        _db.PracticeAttempts.Add(attempt);

        foreach (var fb in request.Feedbacks)
        {
            if (!Enum.TryParse<FeedbackType>(fb.Type, true, out var fbType)) continue;
            _db.AIFeedbacks.Add(new AIFeedback
            {
                Id = Guid.NewGuid(), AttemptId = attempt.Id,
                FeedbackType = fbType, Score = fb.Score,
                Message = fb.Message, KeypointData = null,
                CreatedAt = DateTime.UtcNow
            });
        }

        session.TotalAttempts++;

        var signProgress = await _db.SignProgresses
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == session.SignId);

        if (signProgress == null)
        {
            signProgress = new SignProgress
            {
                Id = Guid.NewGuid(), UserId = userId, SignId = session.SignId,
                AttemptCount = 1, IsMastered = request.OverallScore >= 0.8f,
                LastPracticedAt = DateTime.UtcNow
            };
            _db.SignProgresses.Add(signProgress);
        }
        else
        {
            signProgress.AttemptCount++;
            signProgress.LastPracticedAt = DateTime.UtcNow;
            if (request.OverallScore >= 0.8f) signProgress.IsMastered = true;
        }

        await _streakService.RecordActivityAsync(userId);
        await _db.SaveChangesAsync();

        // --- Gemini AI Feedback (Pro plan only) ---
        string? geminiFeedback = null;
        var activeSub = await _db.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow);

        if (activeSub?.Plan.Type == PlanType.Pro)
        {
            var previousAttempt = await _db.PracticeAttempts
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
