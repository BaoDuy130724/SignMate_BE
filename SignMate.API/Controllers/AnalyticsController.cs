using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Analytics.Queries.GetAdminAlerts;
using SignMate.Application.Features.Analytics.Queries.GetAnalyticsInsight;
using SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;
using SignMate.Application.Features.Analytics.Queries.GetRevenueInsight;

namespace SignMate.API.Controllers;

/// <summary>
/// Phân tích toàn cục cho SuperAdmin: dữ liệu biểu đồ tăng trưởng, phân bổ người dùng và top khóa học,
/// cùng các nhận định AI (insight phân tích/doanh thu) và cảnh báo bất thường.
/// </summary>
[Route("api/analytics")]
[Authorize(Roles = "SuperAdmin")]
public class AnalyticsController : BaseApiController
{
    /// <summary>Số liệu phân tích toàn cục. <c>GET /api/analytics</c>.</summary>
    [HttpGet]
    public async Task<IActionResult> GetGlobalAnalytics()
        => Success(await Mediator.Send(new GetGlobalAnalyticsQuery()));

    /// <summary>Nhận định AI từ số liệu phân tích. <c>GET /api/analytics/insight?forceRefresh=</c>.</summary>
    [HttpGet("insight")]
    public async Task<IActionResult> GetAnalyticsInsight([FromQuery] bool forceRefresh = false)
        => Success(await Mediator.Send(new GetAnalyticsInsightQuery(forceRefresh)));

    /// <summary>Nhận định AI về doanh thu. <c>GET /api/analytics/revenue-insight?forceRefresh=</c>.</summary>
    [HttpGet("revenue-insight")]
    public async Task<IActionResult> GetRevenueInsight([FromQuery] bool forceRefresh = false)
        => Success(await Mediator.Send(new GetRevenueInsightQuery(forceRefresh)));

    /// <summary>Cảnh báo bất thường (rule-based). <c>GET /api/analytics/alerts</c>.</summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAdminAlerts()
        => Success(await Mediator.Send(new GetAdminAlertsQuery()));
}
