using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Queries.GetCenterMembers;

/// <summary>
/// Truy vấn danh sách thành viên của trung tâm theo vai trò. Dùng chung cho danh sách giáo viên
/// (<c>GET /api/centers/{id}/teachers</c>) và học viên (<c>GET /api/centers/{id}/students</c>).
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="Role">Vai trò cần lọc.</param>
public record GetCenterMembersQuery(int CenterId, UserRole Role) : IQuery<List<UserProfileDto>>;
