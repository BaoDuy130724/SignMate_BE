using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Features.Practice.Commands.SubmitAttempt;

/// <summary>
/// Lệnh nộp một lượt luyện tập kèm video để AI chấm điểm — <c>POST /api/practice/attempt</c>.
/// Controller bóc <c>IFormFile</c> thành stream để tầng Application không phụ thuộc kiểu của ASP.NET.
/// </summary>
/// <param name="UserId">Id người luyện tập lấy từ JWT.</param>
/// <param name="SessionId">Id phiên luyện tập đang mở.</param>
/// <param name="VideoStream">Luồng dữ liệu video lượt thử.</param>
/// <param name="FileName">Tên tệp video (dùng khi lưu blob).</param>
public record SubmitAttemptCommand(int UserId, int SessionId, Stream VideoStream, string FileName)
    : ICommand<AttemptResponse>;
