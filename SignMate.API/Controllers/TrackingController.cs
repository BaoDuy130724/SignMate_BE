using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Features.Tracking.Queries.GenerateReport;
using SignMate.Application.Features.Tracking.Queries.GetClassTrackingStats;

namespace SignMate.API.Controllers;

/// <summary>
/// Theo dõi hiệu năng học tập của học viên và xuất báo cáo định kỳ cho trung tâm.
/// </summary>
[Route("api/tracking")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class TrackingController : BaseApiController
{
    /// <summary>Thống kê hiệu năng học viên trong lớp. <c>GET /api/tracking/classes/{classId}/students</c>.</summary>
    [HttpGet("classes/{classId:int}/students")]
    public async Task<IActionResult> GetClassStats(int classId)
        => Success(await Mediator.Send(new GetClassTrackingStatsQuery(classId)));

    /// <summary>Tạo báo cáo định kỳ trung tâm. <c>GET /api/tracking/centers/{centerId}/reports</c>.</summary>
    [HttpGet("centers/{centerId:int}/reports")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GenerateReport(int centerId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Success(await Mediator.Send(new GenerateTrackingReportQuery(centerId, from, to)));
}
