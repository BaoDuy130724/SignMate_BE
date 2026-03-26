using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class StreakService : IStreakService
{
    private readonly ISignMateDbContext _db;

    public StreakService(ISignMateDbContext db) => _db = db;

    public async Task RecordActivityAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = await _db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null)
        {
            _db.Streaks.Add(new Streak
            {
                Id = Guid.NewGuid(), UserId = userId,
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

        await _db.SaveChangesAsync();
    }

    public async Task<Streak?> GetStreakAsync(Guid userId)
    {
        return await _db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
    }
}
