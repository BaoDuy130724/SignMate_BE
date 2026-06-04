using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetPlans;

/// <summary>
/// Truy vấn danh sách gói cước (cả B2C lẫn B2B) để app hiển thị màn hình chọn gói —
/// <c>GET /api/plans</c>.
/// </summary>
public record GetPlansQuery : IQuery<List<SubscriptionPlanDto>>;
