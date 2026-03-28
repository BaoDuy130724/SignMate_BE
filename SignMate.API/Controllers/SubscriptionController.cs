using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subService;

    public SubscriptionController(ISubscriptionService subService)
        => _subService = subService;

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
        => Ok(await _subService.GetPlansAsync());

    [Authorize]
    [HttpPost("subscription/subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var res = await _subService.SubscribeAsync(userId, request);
        return res.Success ? Ok(res) : BadRequest(new { message = res.Message });
    }

    [Authorize]
    [HttpGet("subscription/me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sub = await _subService.GetMySubscriptionAsync(userId);
        return sub == null ? NotFound() : Ok(sub);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("subscription/all")]
    public async Task<IActionResult> GetAllSubscriptions()
        => Ok(await _subService.GetAllSubscriptionsAsync());
}
