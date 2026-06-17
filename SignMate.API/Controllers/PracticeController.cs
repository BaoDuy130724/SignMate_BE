using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Features.Practice.Commands.EndSession;
using SignMate.Application.Features.Practice.Commands.StartSession;
using SignMate.Application.Features.Practice.Commands.SubmitAttempt;
using SignMate.Application.Features.Practice.Queries.GetAttemptFeedback;
using SignMate.Application.Features.Practice.Queries.GetPracticeHistory;

namespace SignMate.API.Controllers;

/// <summary>
/// Luồng luyện tập ký hiệu: mở phiên, nộp lượt thử (kèm video hoặc kết quả chấm sẵn),
/// kết thúc phiên và xem lịch sử.
/// </summary>
[Route("api/practice")]
[Authorize]
public class PracticeController : BaseApiController
{
    /// <summary>Bắt đầu một phiên luyện tập. <c>POST /api/practice/session/start</c>.</summary>
    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new StartSessionCommand(userId, request.SignId));
        return Success(result);
    }

    /// <summary>Nộp lượt thử kèm video để AI chấm điểm. <c>POST /api/practice/attempt</c>.</summary>
    [HttpPost("attempt")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitAttempt([FromForm] int sessionId, IFormFile video)
    {
        if (video is null || video.Length == 0)
            throw new BadRequestException("File video không được để trống.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var stream = video.OpenReadStream();

        var result = await Mediator.Send(new SubmitAttemptCommand(userId, sessionId, stream, video.FileName));
        return Success(result);
    }

    /// <summary>Kết thúc một phiên luyện tập. <c>POST /api/practice/session/end</c>.</summary>
    [HttpPost("session/end")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new EndSessionCommand(userId, request.SessionId));
        return Success("Đã kết thúc phiên luyện tập.");
    }

    /// <summary>
    /// Nhận xét chi tiết Gemini cho một lượt thử (gói Pro/B2B). <c>GET /api/practice/attempt/{attemptId}/feedback</c>.
    /// Tách khỏi luồng chấm điểm để điểm hiện ngay; app gọi sau khi đã có điểm. Trả <c>feedback=null</c>
    /// nếu người dùng không thuộc gói Pro/B2B (hoặc Gemini tạm lỗi).
    /// </summary>
    [HttpGet("attempt/{attemptId:int}/feedback")]
    public async Task<IActionResult> GetAttemptFeedback(int attemptId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetAttemptFeedbackQuery(userId, attemptId));
        return Success(result);
    }

    /// <summary>Lịch sử luyện tập cho một ký hiệu. <c>GET /api/practice/history/{signId}</c>.</summary>
    [HttpGet("history/{signId:int}")]
    public async Task<IActionResult> GetHistory(int signId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetPracticeHistoryQuery(userId, signId));
        return Success(result);
    }

}
