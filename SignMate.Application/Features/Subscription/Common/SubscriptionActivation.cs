using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Common;

/// <summary>
/// Tiện ích dùng chung cho luồng kích hoạt gói cước. Tách riêng để việc "hủy hiệu lực các gói
/// đang active của người dùng" được tái sử dụng nhất quán giữa kích hoạt gói Free và xác nhận
/// thanh toán gói trả phí (DRY) — đảm bảo mỗi người dùng chỉ có tối đa một gói active.
/// </summary>
public static class SubscriptionActivation
{
    /// <summary>
    /// Đánh dấu tất cả gói đang active của người dùng thành không hiệu lực (chưa lưu DB —
    /// caller gọi SaveChanges trong cùng transaction logic để đảm bảo Atomicity).
    /// </summary>
    public static async Task DeactivateActiveSubscriptionsAsync(
        IUnitOfWork unitOfWork, int userId, CancellationToken cancellationToken)
    {
        var actives = await unitOfWork.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var sub in actives)
            sub.IsActive = false;
    }

    /// <summary>
    /// Tự động gán gói B2B cho một học viên thuộc trung tâm.
    /// Hủy các gói cũ trước rồi tạo gói B2B mới (chưa SaveChanges — caller bao transaction).
    /// Không làm gì nếu không tìm thấy gói B2B trong hệ thống.
    /// </summary>
    public static async Task AutoAssignB2BAsync(
        IUnitOfWork unitOfWork, User user, CancellationToken cancellationToken)
    {
        var b2bPlan = await unitOfWork.Repository<SubscriptionPlan>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Type == PlanType.B2B, cancellationToken);

        if (b2bPlan is null) return;

        await DeactivateActiveSubscriptionsAsync(unitOfWork, user.Id, cancellationToken);

        var now = DateTime.UtcNow;
        await unitOfWork.Repository<UserSubscription>().AddAsync(new UserSubscription
        {
            User = user,
            PlanId = b2bPlan.Id,
            StartDate = now,
            EndDate = now.AddYears(10), // không hết hạn thực tế — gắn với center
            IsActive = true
        });
    }
}
