using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class StreakService : IStreakService
{
    private readonly IUnitOfWork _unitOfWork;

    public StreakService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task RecordActivityAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = await _unitOfWork.Repository<Streak>().Query()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null)
        {
            await _unitOfWork.Repository<Streak>().AddAsync(new Streak
            {
                Id = 0, UserId = userId,
                CurrentStreak = 1, LongestStreak = 1, LastActiveDate = today
            });
        }
        else if (streak.LastActiveDate == today) { return; }
        else if (streak.LastActiveDate == today.AddDays(-1))
        {
            streak.CurrentStreak++;
            streak.LastActiveDate = today;
            if (streak.CurrentStreak > streak.LongestStreak)
                streak.LongestStreak = streak.CurrentStreak;
        }
        else
        {
            streak.CurrentStreak = 1;
            streak.LastActiveDate = today;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Streak?> GetStreakAsync(int userId)
    {
        return await _unitOfWork.Repository<Streak>().Query()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
