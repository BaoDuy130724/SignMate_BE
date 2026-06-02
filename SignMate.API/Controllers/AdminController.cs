using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.DTOs.Common;
using SignMate.Application.Features.Admin;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _mediator.Send(new GetSystemDashboardQuery());
        return Ok(ApiResponse<SystemDashboardDto>.SuccessResult(result, "Dashboard loaded successfully."));
    }
}
