using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.Refresh;

/// <summary>
/// Lệnh làm mới phiên đăng nhập bằng refresh token (rotation: thu hồi token cũ, cấp token mới) —
/// <c>POST /api/auth/refresh</c>.
/// </summary>
public record RefreshCommand(string RefreshToken) : ICommand<TokenResponse>;
