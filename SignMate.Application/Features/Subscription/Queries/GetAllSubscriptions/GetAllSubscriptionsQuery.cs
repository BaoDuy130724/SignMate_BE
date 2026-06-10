using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;

/// <summary>
/// Truy vấn danh sách subscription — <c>GET /api/subscription/all</c>.
/// SuperAdmin thấy tất cả; CenterAdmin chỉ thấy subscription của student trong center mình.
/// </summary>
/// <param name="CallerCenterId">Id trung tâm của caller; null = SuperAdmin (toàn bộ).</param>
public record GetAllSubscriptionsQuery(int? CallerCenterId = null) : IQuery<List<SubscriptionListItemDto>>;
