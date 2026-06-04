using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Notifications.Commands.MarkAsRead;

/// <summary>
/// Lệnh đánh dấu một thông báo của người dùng là đã đọc — <c>PUT /api/notifications/{id}/read</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT (đảm bảo chỉ đọc thông báo của chính mình).</param>
/// <param name="NotificationId">Id thông báo cần đánh dấu.</param>
public record MarkNotificationAsReadCommand(int UserId, int NotificationId) : ICommand<Unit>;
