using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Common;

/// <summary>
/// Quy tắc dùng chung xác định một gói cước được hưởng tính năng tầng nào. Tách riêng để các cổng
/// kiểm tra tính năng (vd Gemini feedback chi tiết) dùng cùng một định nghĩa, tránh chỗ thì tính
/// đúng chỗ thì sót gói (DRY).
/// </summary>
internal static class PlanEntitlements
{
    /// <summary>
    /// Gói được hưởng các tính năng AI cao cấp (Gemini feedback chi tiết, coaching realtime).
    /// Bao gồm Pro và B2B — học viên trung tâm trả phí theo đầu người nên được trải nghiệm ngang Pro.
    /// LƯU Ý: đây là phân tầng <b>tính năng</b>, khác với việc đếm chuyển đổi B2C trên dashboard
    /// (nơi B2B cố ý không được tính là "paid B2C").
    /// </summary>
    public static bool HasProFeatures(PlanType type) => type is PlanType.Pro or PlanType.B2B;
}
