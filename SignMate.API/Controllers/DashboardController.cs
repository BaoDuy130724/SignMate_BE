using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Dashboard.Queries.GetDashboardSummary;
using SignMate.Application.Features.Dashboard.Queries.GetProgressStats;

namespace SignMate.API.Controllers;

/// <summary>
/// Cung cấp dữ liệu tổng quan &amp; thống kê tiến độ cho trang chủ học viên.
/// </summary>
[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// Dữ liệu tổng quan trang chủ (streak, độ chính xác, gợi ý, deadline). <c>GET /api/dashboard</c>.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetDashboardSummaryQuery(userId));
        return Success(result);
    }

    /// <summary>
    /// Thống kê tiến độ học tập chi tiết. <c>GET /api/dashboard/progress</c>.
    /// </summary>
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgressStats()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetProgressStatsQuery(userId));
        return Success(result);
    }
}
