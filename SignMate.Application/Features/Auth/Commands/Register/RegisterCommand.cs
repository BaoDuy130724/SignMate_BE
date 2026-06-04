using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.Register;

/// <summary>
/// Lệnh tạo tài khoản học viên mới sau khi xác thực OTP — <c>POST /api/auth/register</c>.
/// </summary>
public record RegisterCommand(RegisterRequest Request) : ICommand<TokenResponse>;
