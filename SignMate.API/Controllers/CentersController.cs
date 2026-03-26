using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/centers")]
[Authorize(Roles = "SuperAdmin,CenterAdmin")]
public class CentersController : ControllerBase
{
    private readonly ICenterService _centerService;

    public CentersController(ICenterService centerService)
        => _centerService = centerService;

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCenters()
        => Ok(await _centerService.GetCentersAsync());

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenter([FromBody] CenterDto request)
    {
        var center = await _centerService.CreateCenterAsync(request);
        return CreatedAtAction(nameof(GetCenters), new { id = center.Id }, center);
    }

    [HttpGet("{id:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid id)
        => Ok(await _centerService.GetCenterDashboardAsync(id));
}
