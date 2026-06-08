using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Truy vấn chi tiết một người dùng theo Id (dành cho SuperAdmin) — <c>GET /api/users/{id}</c>.
/// </summary>
/// <param name="Id">Khóa chính của người dùng.</param>
public record GetUserByIdQuery(int Id) : IQuery<UserProfileDto>;
