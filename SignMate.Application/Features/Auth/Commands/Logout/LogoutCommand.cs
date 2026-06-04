using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Lệnh đăng xuất: thu hồi refresh token của phiên hiện tại — <c>POST /api/auth/logout</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT (đảm bảo chỉ thu hồi token của chính mình).</param>
/// <param name="RefreshToken">Refresh token cần thu hồi.</param>
public record LogoutCommand(int UserId, string RefreshToken) : ICommand<Unit>;
