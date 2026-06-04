using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Notifications.Commands.MarkAsRead;

/// <summary>
/// Handler cho <see cref="MarkNotificationAsReadCommand"/>: tìm thông báo thuộc đúng người dùng
/// rồi bật cờ đã đọc. Idempotent — nếu thông báo đã ở trạng thái đã đọc thì không ghi lại.
/// </summary>
public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(MarkNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notification = await repo.Query()
            .FirstOrDefaultAsync(n => n.Id == command.NotificationId && n.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), command.NotificationId);

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
