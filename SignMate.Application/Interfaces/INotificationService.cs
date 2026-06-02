using SignMate.Application.DTOs.Notification;

namespace SignMate.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationPagedResponse> GetNotificationsAsync(int userId, int page, int pageSize);
    Task MarkAsReadAsync(int userId, int notificationId);
}
