using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Commands.UpdatePlan;

/// <summary>
/// Lệnh cập nhật gói cước (SuperAdmin) — <c>PUT /api/plans/{id}</c>.
/// </summary>
public record UpdatePlanCommand(int Id, UpdatePlanRequest Request) : ICommand<SubscriptionPlanDto>;
