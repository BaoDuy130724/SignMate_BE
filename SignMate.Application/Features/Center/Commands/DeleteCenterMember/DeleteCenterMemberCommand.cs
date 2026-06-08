using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Center.Commands.DeleteCenterMember;

/// <summary>
/// Lệnh xóa/gỡ bỏ thành viên ra khỏi trung tâm — <c>DELETE /api/centers/{centerId}/members/{userId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="UserId">Id thành viên cần xóa.</param>
/// <param name="CurrentUserId">Id CenterAdmin thực hiện yêu cầu.</param>
public record DeleteCenterMemberCommand(int CenterId, int UserId, int CurrentUserId) : ICommand<Unit>;
