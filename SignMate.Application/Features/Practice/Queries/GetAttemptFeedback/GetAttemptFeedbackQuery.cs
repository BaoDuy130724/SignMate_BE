using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Features.Practice.Queries.GetAttemptFeedback;

/// <summary>
/// Lấy nhận xét chi tiết Gemini cho một lượt thử — <c>GET /api/practice/attempt/{attemptId}/feedback</c>.
/// Tách khỏi luồng chấm điểm để điểm số hiện ra ngay; nhận xét (Pro/B2B) sinh theo yêu cầu ở đây.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT (để kiểm tra sở hữu lượt thử).</param>
/// <param name="AttemptId">Id lượt thử cần nhận xét.</param>
public record GetAttemptFeedbackQuery(int UserId, int AttemptId) : IQuery<AttemptFeedbackResponse>;
