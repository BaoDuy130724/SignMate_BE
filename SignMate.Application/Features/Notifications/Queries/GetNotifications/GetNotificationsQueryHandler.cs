using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Notification;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// Handler đọc danh sách thông báo của người dùng. Đếm tổng và phân trang bằng EF Core,
/// projection trực tiếp sang DTO để tránh tải thừa cột không dùng tới.
/// </summary>
public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<NotificationPagedResponse> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = _unitOfWork.Repository<Notification>().Query()
            .Where(n => n.UserId == query.UserId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new NotificationPagedResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
