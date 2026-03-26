using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
        => _onboardingService = onboardingService;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] OnboardingRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await _onboardingService.SubmitOnboardingAsync(userId, request);
        return res.Success ? Ok(res) : BadRequest();
    }
}
