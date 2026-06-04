using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetMyAchievements;

/// <summary>
/// Truy vấn danh sách thành tích người dùng đã đạt được, mới nhất trước —
/// <c>GET /api/users/me/achievements</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
public record GetMyAchievementsQuery(int UserId) : IQuery<List<AchievementDto>>;
