using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.StudentTracking;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class TrackingController : ControllerBase
{
    private readonly IStudentTrackingService _trackingService;

    public TrackingController(IStudentTrackingService trackingService)
        => _trackingService = trackingService;

    [HttpGet("classes/{classId:int}/students")]
    public async Task<IActionResult> GetClassStats(int classId)
        => Ok(await _trackingService.GetClassTrackingStatsAsync(classId));

    [HttpGet("centers/{centerId:int}/reports")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GenerateReport(int centerId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _trackingService.GenerateTrackingReportAsync(centerId, from, to));
}
