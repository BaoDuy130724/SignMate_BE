using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Center.Commands.UpdateCenterMember;

/// <summary>
/// Lệnh cập nhật thông tin thành viên trung tâm (FullName) — <c>PUT /api/centers/{centerId}/members/{userId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="UserId">Id thành viên cần sửa.</param>
/// <param name="CurrentUserId">Id CenterAdmin thực hiện yêu cầu.</param>
/// <param name="Request">Thông tin cập nhật mới.</param>
public record UpdateCenterMemberCommand(int CenterId, int UserId, int CurrentUserId, UpdateCenterMemberRequest Request)
    : ICommand<UserProfileDto>;
