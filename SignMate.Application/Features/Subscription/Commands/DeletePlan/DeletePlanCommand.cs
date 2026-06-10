using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Subscription.Commands.DeletePlan;

/// <summary>
/// Lệnh xóa gói cước (SuperAdmin) — <c>DELETE /api/plans/{id}</c>.
/// </summary>
public record DeletePlanCommand(int Id) : ICommand<Unit>;
