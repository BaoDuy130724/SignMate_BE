using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Common;
using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Features.Onboarding.Commands.SubmitOnboarding;

namespace SignMate.API.Controllers;

/// <summary>
/// Lưu cấu hình cá nhân hóa lộ trình học của học viên ở bước onboarding.
/// </summary>
[Route("api/onboarding")]
[Authorize]
public class OnboardingController : BaseApiController
{
    /// <summary>
    /// Gửi lựa chọn mục tiêu &amp; trình độ học tập. <c>POST /api/onboarding</c>.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OnboardingResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Submit([FromBody] OnboardingRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new SubmitOnboardingCommand(userId, request));
        return Success(result, "Lưu cấu hình lộ trình thành công.");
    }
}
