using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Lệnh yêu cầu mã OTP khôi phục mật khẩu — <c>POST /api/auth/forgot-password</c>.
/// </summary>
public record ForgotPasswordCommand(ForgotPasswordRequest Request) : ICommand<Unit>;
