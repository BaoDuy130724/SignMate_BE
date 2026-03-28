using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "SuperAdmin")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGlobalAnalytics()
        => Ok(await analyticsService.GetGlobalAnalyticsAsync());
}
