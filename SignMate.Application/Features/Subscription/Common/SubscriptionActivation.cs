using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Common;

/// <summary>
/// Tiện ích dùng chung cho luồng kích hoạt gói cước. Tách riêng để việc "hủy hiệu lực các gói
/// đang active của người dùng" được tái sử dụng nhất quán giữa kích hoạt gói Free và xác nhận
/// thanh toán gói trả phí (DRY) — đảm bảo mỗi người dùng chỉ có tối đa một gói active.
/// </summary>
internal static class SubscriptionActivation
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
}
