using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Notifications.Commands.MarkAsRead;
using SignMate.Application.Features.Notifications.Queries.GetNotifications;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý thông báo đẩy tới người dùng (danh sách + đánh dấu đã đọc).
/// </summary>
[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    /// <summary>
    /// Lấy danh sách thông báo phân trang của người dùng hiện tại. <c>GET /api/notifications</c>.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetNotificationsQuery(userId, page, pageSize));
        return Success(result);
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc. <c>PUT /api/notifications/{id}/read</c>.
    /// </summary>
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new MarkNotificationAsReadCommand(userId, id));
        return Success("Đã đánh dấu thông báo là đã đọc.");
    }
}
