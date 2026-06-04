using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Notification;

namespace SignMate.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// Truy vấn danh sách thông báo của người dùng theo trang (mới nhất trước) — <c>GET /api/notifications</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
/// <param name="Page">Số trang (bắt đầu từ 1).</param>
/// <param name="PageSize">Số bản ghi mỗi trang.</param>
public record GetNotificationsQuery(int UserId, int Page, int PageSize)
    : IQuery<NotificationPagedResponse>;
