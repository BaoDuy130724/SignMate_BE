using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Features.Practice.Queries.GetPracticeHistory;

/// <summary>
/// Truy vấn lịch sử luyện tập của người dùng cho một ký hiệu — <c>GET /api/practice/history/{signId}</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
/// <param name="SignId">Id ký hiệu cần xem lịch sử.</param>
public record GetPracticeHistoryQuery(int UserId, int SignId) : IQuery<List<PracticeHistoryDto>>;
