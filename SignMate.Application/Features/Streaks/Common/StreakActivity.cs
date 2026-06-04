using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Streaks.Common;

/// <summary>
/// Tiện ích dùng chung cho việc ghi nhận "có hoạt động học tập trong ngày" và cập nhật chuỗi streak.
/// Tách riêng để luồng luyện tập (Practice) và chơi game (Games) tái sử dụng cùng một quy tắc cộng
/// dồn streak (DRY), tránh lệch logic giữa các feature.
/// </summary>
internal static class StreakActivity
{
    /// <summary>
    /// Cập nhật streak của người dùng theo hoạt động hôm nay (chưa <c>SaveChanges</c> — caller lưu trong
    /// cùng đơn vị công việc để đảm bảo Atomicity). Quy tắc:
    /// chưa có bản ghi → khởi tạo streak = 1; hoạt động lại trong ngày → giữ nguyên (idempotent);
    /// hoạt động đúng hôm sau ngày gần nhất → +1 và nới <c>LongestStreak</c>; cách quãng → reset về 1.
    /// </summary>
    /// <returns>Bản ghi streak đã cập nhật để caller đọc <c>CurrentStreak</c> hiện tại.</returns>
    public static async Task<Streak> RecordAsync(
        IUnitOfWork unitOfWork, int userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = await unitOfWork.Repository<Streak>().Query()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (streak is null)
        {
            streak = new Streak
            {
                UserId = userId,
                CurrentStreak = 1,
                LongestStreak = 1,
                LastActiveDate = today
            };
            await unitOfWork.Repository<Streak>().AddAsync(streak);
        }
        else if (streak.LastActiveDate == today)
        {
            // Đã ghi nhận hoạt động hôm nay — không cộng dồn lần nữa.
        }
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

        return streak;
    }
}
