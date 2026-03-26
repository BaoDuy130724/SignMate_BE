using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/vocabulary")]
[Authorize] // Ideally we would specify roles: [Authorize(Roles = "Admin,Teacher,SuperAdmin")]
public class VocabularyController : ControllerBase
{
    private readonly ICourseService _courseService;

    public VocabularyController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpPost("set-reference")]
    [Authorize(Roles = "SuperAdmin,CenterAdmin,Teacher")]
    public async Task<IActionResult> SetReference([FromBody] SetReferenceRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ReferenceKeypointData))
            return BadRequest(new { message = "Invalid valid ReferenceKeypointData payload." });

        var success = await _courseService.SetSignReferenceAsync(request);
        
        if (!success)
            return NotFound(new { message = "Sign not found to attach reference." });

        return Ok(new { message = "Reference keypoints set successfully." });
    }
}
