using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Notification;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<NotificationPagedResponse> GetNotificationsAsync(int userId, int page, int pageSize)
    {
        var query = _unitOfWork.Repository<Notification>().Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);
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

    public async Task MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _unitOfWork.Repository<Notification>().Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
