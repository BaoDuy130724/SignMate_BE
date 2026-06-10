using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Commands.CreatePlan;

/// <summary>
/// Lệnh tạo gói cước mới (SuperAdmin) — <c>POST /api/plans</c>.
/// </summary>
public record CreatePlanCommand(CreatePlanRequest Request) : ICommand<SubscriptionPlanDto>;
