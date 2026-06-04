using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;

/// <summary>
/// Truy vấn toàn bộ lượt đăng ký gói (dành cho SuperAdmin) — <c>GET /api/subscription/all</c>.
/// </summary>
public record GetAllSubscriptionsQuery : IQuery<List<SubscriptionListItemDto>>;
