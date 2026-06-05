using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản trị hệ thống cấp cao (SuperAdmin): dashboard tổng quan vận hành toàn nền tảng.
/// </summary>
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : BaseApiController
{
    /// <summary>Dashboard tổng quan hệ thống. <c>GET /api/admin/dashboard</c>.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
        => Success(await Mediator.Send(new GetSystemDashboardQuery()), "Dashboard loaded successfully.");
}
