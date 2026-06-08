using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Lệnh xóa người dùng do SuperAdmin thực hiện — <c>DELETE /api/users/{id}</c>.
/// </summary>
/// <param name="Id">Người dùng cần xóa.</param>
/// <param name="CurrentUserId">Id quản trị viên đang đăng nhập (để chặn tự xóa).</param>
public record DeleteUserCommand(int Id, int CurrentUserId) : ICommand;
