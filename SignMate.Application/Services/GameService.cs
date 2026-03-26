using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Game;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class GameService : IGameService
{
    private readonly ISignMateDbContext _db;
    private readonly IStreakService _streakService;

    public GameService(ISignMateDbContext db, IStreakService streakService)
    {
        _db = db;
        _streakService = streakService;
    }

    public async Task<Guid> StartGameAsync(Guid userId, StartGameRequest request)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(), UserId = userId, GameType = request.GameType,
            PlayedAt = DateTime.UtcNow, XpEarned = 0
        };
        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();
        return session.Id;
    }

    public async Task<GameResultResponse> CompleteGameAsync(Guid userId, CompleteGameRequest request)
    {
        var session = await _db.GameSessions.FindAsync(request.SessionId);
        if (session != null && session.UserId == userId)
        {
            session.XpEarned = request.Score; // simple mapping
        }

        var user = await _db.Users.FindAsync(userId);
        if (user != null) user.XpPoints += request.Score;

        await _streakService.RecordActivityAsync(userId);
        await _db.SaveChangesAsync();

        var streak = await _streakService.GetStreakAsync(userId);

        return new GameResultResponse
        {
            XpEarned = request.Score,
            TotalXp = user?.XpPoints ?? 0,
            StreakUpdated = streak?.CurrentStreak ?? 0
        };
    }
}
