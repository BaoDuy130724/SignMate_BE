using SignMate.Application.DTOs.Notification;

namespace SignMate.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationPagedResponse> GetNotificationsAsync(Guid userId, int page, int pageSize);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
}
