using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Truy vấn danh sách người dùng — <c>GET /api/users</c>.
/// SuperAdmin thấy tất cả; CenterAdmin chỉ thấy user thuộc center của mình.
/// </summary>
/// <param name="Role">Tên vai trò để lọc (tùy chọn).</param>
/// <param name="CallerCenterId">Id trung tâm của caller; null = SuperAdmin (không lọc).</param>
public record GetAllUsersQuery(string? Role, int? CallerCenterId = null) : IQuery<List<UserProfileDto>>;
