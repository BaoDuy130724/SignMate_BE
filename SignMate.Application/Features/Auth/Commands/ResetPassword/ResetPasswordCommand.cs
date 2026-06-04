using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Lệnh đặt lại mật khẩu bằng mã OTP đã nhận qua email — <c>POST /api/auth/reset-password</c>.
/// </summary>
public record ResetPasswordCommand(ResetPasswordRequest Request) : ICommand<Unit>;
