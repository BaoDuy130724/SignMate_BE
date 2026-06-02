using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Common;
using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Features.Onboarding.Commands;
using System.Security.Claims;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OnboardingResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Submit([FromBody] OnboardingRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await _mediator.Send(new SubmitOnboardingCommand(userId, request));
        return res.Success ? Ok(res) : BadRequest(res);
    }
}
