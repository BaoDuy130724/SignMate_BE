using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetMyProfile;

/// <summary>
/// Truy vấn hồ sơ đầy đủ của người dùng hiện tại (kèm gói cước, streak, XP, thống kê học tập) —
/// <c>GET /api/users/me</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
public record GetMyProfileQuery(int UserId) : IQuery<UserProfileDto>;
