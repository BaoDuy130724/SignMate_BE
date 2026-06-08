using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Lệnh cập nhật người dùng bất kỳ (partial update) do SuperAdmin thực hiện — <c>PUT /api/users/{id}</c>.
/// </summary>
/// <param name="Id">Khóa chính của người dùng cần sửa.</param>
/// <param name="Request">Các trường cần cập nhật (null = giữ nguyên).</param>
public record UpdateUserCommand(int Id, UpdateUserRequest Request) : ICommand<UserProfileDto>;
