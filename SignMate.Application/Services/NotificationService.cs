using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Notification;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Services;

public class NotificationService : INotificationService
{
    private readonly ISignMateDbContext _db;

    public NotificationService(ISignMateDbContext db) => _db = db;

    public async Task<NotificationPagedResponse> GetNotificationsAsync(Guid userId, int page, int pageSize)
    {
        var query = _db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt);
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id, Title = n.Title, Body = n.Body,
                Type = n.Type, IsRead = n.IsRead, CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new NotificationPagedResponse
        {
            Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}
