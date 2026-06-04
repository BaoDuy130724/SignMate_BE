using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Features.Practice.Commands.ReportResult;

/// <summary>
/// Lệnh báo kết quả một lượt luyện đã được chấm sẵn phía client (không tải video) —
/// <c>POST /api/practice/report-result</c>. Người dùng gói Pro được nhận thêm phản hồi chi tiết từ Gemini.
/// </summary>
/// <param name="UserId">Id người luyện tập lấy từ JWT.</param>
/// <param name="Request">Phiên, điểm tổng và danh sách phản hồi DTW từ client.</param>
public record ReportResultCommand(int UserId, ReportResultRequest Request) : ICommand<AttemptResponse>;
