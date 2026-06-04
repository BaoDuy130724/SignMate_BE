using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Common;

/// <summary>
/// Tiện ích dùng chung cập nhật tiến độ luyện một ký hiệu sau mỗi lượt thử. Tái sử dụng giữa luồng
/// nộp video (SubmitAttempt) và báo kết quả chấm sẵn (ReportResult) để quy tắc cộng số lần thử &amp;
/// đánh dấu thành thạo luôn nhất quán (DRY). Không <c>SaveChanges</c> — caller lưu trong cùng transaction.
/// </summary>
internal static class SignProgressUpdater
{
    /// <summary>Ngưỡng điểm tổng để coi một ký hiệu là đã thành thạo.</summary>
    private const float MasteryThreshold = 0.8f;

    /// <summary>
    /// Tạo mới hoặc cập nhật <see cref="SignProgress"/>: tăng số lần thử, ghi nhận thời điểm luyện,
    /// và bật cờ thành thạo khi điểm đạt ngưỡng.
    /// </summary>
    public static async Task UpsertAsync(
        IUnitOfWork unitOfWork, int userId, int signId, float overallScore, CancellationToken cancellationToken)
    {
        var mastered = overallScore >= MasteryThreshold;

        var progress = await unitOfWork.Repository<SignProgress>().Query()
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == signId, cancellationToken);

        if (progress is null)
        {
            await unitOfWork.Repository<SignProgress>().AddAsync(new SignProgress
            {
                UserId = userId,
                SignId = signId,
                AttemptCount = 1,
                IsMastered = mastered,
                LastPracticedAt = DateTime.UtcNow
            });
        }
        else
        {
            progress.AttemptCount++;
            progress.LastPracticedAt = DateTime.UtcNow;
            if (mastered) progress.IsMastered = true;
        }
    }
}
