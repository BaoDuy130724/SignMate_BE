using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Queries.GetMyStreak;

/// <summary>
/// Truy vấn thông số streak (chuỗi ngày học liên tục) của người dùng hiện tại —
/// <c>GET /api/users/me/streak</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
public record GetMyStreakQuery(int UserId) : IQuery<StreakDto>;
