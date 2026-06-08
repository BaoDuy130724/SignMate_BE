using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Center.Queries.GetCenterMember;

/// <summary>
/// Truy vấn chi tiết một thành viên (giáo viên/học viên) của trung tâm — <c>GET /api/centers/{centerId}/members/{userId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="UserId">Id thành viên cần xem.</param>
/// <param name="CurrentUserId">Id người dùng hiện tại thực hiện yêu cầu (để check center).</param>
public record GetCenterMemberQuery(int CenterId, int UserId, int CurrentUserId) : IQuery<UserProfileDto>;
