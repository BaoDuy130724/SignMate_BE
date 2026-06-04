using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Progress;

namespace SignMate.Application.Features.Progress.Commands.UpdateLessonProgress;

/// <summary>
/// Lệnh cập nhật tiến độ học một bài (xem đến đâu / hoàn thành) — <c>PUT /api/progress/lesson</c>.
/// Khi bài cuối cùng của khóa hoàn thành sẽ đánh dấu enrollment là đã hoàn tất.
/// </summary>
/// <param name="UserId">Id người học lấy từ JWT.</param>
/// <param name="Request">Bài học, trạng thái và thời lượng xem mới ghi nhận.</param>
public record UpdateLessonProgressCommand(int UserId, UpdateLessonProgressRequest Request) : ICommand<Unit>;
