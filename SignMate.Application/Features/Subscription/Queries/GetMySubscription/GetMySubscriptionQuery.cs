using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetMySubscription;

/// <summary>
/// Truy vấn gói cước đang hiệu lực của người dùng hiện tại — <c>GET /api/subscription/me</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
public record GetMySubscriptionQuery(int UserId) : IQuery<MySubscriptionDto>;
