using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Truy vấn danh sách người dùng (dành cho SuperAdmin), có thể lọc theo vai trò —
/// <c>GET /api/users</c>.
/// </summary>
/// <param name="Role">Tên vai trò để lọc (tùy chọn); bỏ trống để lấy tất cả.</param>
public record GetAllUsersQuery(string? Role) : IQuery<List<UserProfileDto>>;
