using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/practice")]
[Authorize]
public class PracticeController : ControllerBase
{
    private readonly IPracticeService _practiceService;

    public PracticeController(IPracticeService practiceService) => _practiceService = practiceService;

    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _practiceService.StartSessionAsync(userId, request));
    }

    [HttpPost("attempt")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitAttempt([FromForm] Guid sessionId, IFormFile video)
    {
        if (video == null || video.Length == 0)
            return BadRequest(new { message = "Video file is required." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        using var stream = video.OpenReadStream();
        return Ok(await _practiceService.SubmitAttemptAsync(userId, sessionId, stream, video.FileName));
    }

    [HttpPost("session/end")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _practiceService.EndSessionAsync(userId, request.SessionId);
        return Ok(new { message = "Session ended." });
    }

    [HttpGet("history/{signId:guid}")]
    public async Task<IActionResult> GetHistory(Guid signId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _practiceService.GetHistoryAsync(userId, signId));
    }

    [HttpPost("report-result")]
    public async Task<IActionResult> ReportResult([FromBody] ReportResultRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _practiceService.ReportResultAsync(userId, request));
    }
}
