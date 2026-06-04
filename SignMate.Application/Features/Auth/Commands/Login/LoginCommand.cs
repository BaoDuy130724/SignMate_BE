using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Lệnh đăng nhập bằng email + mật khẩu, trả về cặp access/refresh token —
/// <c>POST /api/auth/login</c>.
/// </summary>
public record LoginCommand(LoginRequest Request) : ICommand<TokenResponse>;
