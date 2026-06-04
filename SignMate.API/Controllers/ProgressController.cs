using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Progress;
using SignMate.Application.Features.Progress.Commands.UpdateLessonProgress;
using SignMate.Application.Features.Progress.Commands.UpdateSignProgress;

namespace SignMate.API.Controllers;

/// <summary>
/// Cập nhật tiến độ học tập của người dùng: tiến độ bài học và tiến độ luyện ký hiệu.
/// </summary>
[Route("api/progress")]
[Authorize]
public class ProgressController : BaseApiController
{
    /// <summary>Cập nhật tiến độ một bài học. <c>PUT /api/progress/lesson</c>.</summary>
    [HttpPut("lesson")]
    public async Task<IActionResult> UpdateLessonProgress([FromBody] UpdateLessonProgressRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new UpdateLessonProgressCommand(userId, request));
        return Success("Đã cập nhật tiến độ bài học.");
    }

    /// <summary>Cập nhật tiến độ luyện một ký hiệu. <c>PUT /api/progress/sign</c>.</summary>
    [HttpPut("sign")]
    public async Task<IActionResult> UpdateSignProgress([FromBody] UpdateSignProgressRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new UpdateSignProgressCommand(userId, request));
        return Success("Đã cập nhật tiến độ ký hiệu.");
    }
}
