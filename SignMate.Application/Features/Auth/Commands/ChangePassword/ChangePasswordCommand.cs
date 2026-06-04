using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Lệnh đổi mật khẩu khi đã đăng nhập (yêu cầu mật khẩu hiện tại) —
/// <c>POST /api/auth/change-password</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
/// <param name="Request">Mật khẩu hiện tại và mật khẩu mới.</param>
public record ChangePasswordCommand(int UserId, ChangePasswordRequest Request) : ICommand<Unit>;
