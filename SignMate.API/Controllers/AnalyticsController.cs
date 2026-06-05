using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;

namespace SignMate.API.Controllers;

/// <summary>
/// Phân tích toàn cục cho SuperAdmin: dữ liệu biểu đồ tăng trưởng, phân bổ người dùng và top khóa học.
/// </summary>
[Route("api/analytics")]
[Authorize(Roles = "SuperAdmin")]
public class AnalyticsController : BaseApiController
{
    /// <summary>Số liệu phân tích toàn cục. <c>GET /api/analytics</c>.</summary>
    [HttpGet]
    public async Task<IActionResult> GetGlobalAnalytics()
        => Success(await Mediator.Send(new GetGlobalAnalyticsQuery()));
}
