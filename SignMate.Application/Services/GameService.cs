using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Game;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class GameService : IGameService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStreakService _streakService;

    public GameService(IUnitOfWork unitOfWork, IStreakService streakService)
    {
        _unitOfWork = unitOfWork;
        _streakService = streakService;
    }

    public async Task<int> StartGameAsync(int userId, StartGameRequest request)
    {
        var session = new GameSession
        {
            Id = 0, UserId = userId, GameType = request.GameType,
            PlayedAt = DateTime.UtcNow, XpEarned = 0
        };
        await _unitOfWork.Repository<GameSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return session.Id;
    }

    public async Task<GameResultResponse> CompleteGameAsync(int userId, CompleteGameRequest request)
    {
        var session = await _unitOfWork.Repository<GameSession>().GetByIdAsync(request.SessionId);
        if (session != null && session.UserId == userId)
        {
            session.XpEarned = request.Score; // simple mapping
        }

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user != null) user.XpPoints += request.Score;

        await _streakService.RecordActivityAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        var streak = await _streakService.GetStreakAsync(userId);

        return new GameResultResponse
        {
            XpEarned = request.Score,
            TotalXp = user?.XpPoints ?? 0,
            StreakUpdated = streak?.CurrentStreak ?? 0
        };
    }
}
