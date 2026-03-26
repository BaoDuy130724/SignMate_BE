using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/centers/{centerId:guid}/classes")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
        => _classService = classService;

    [HttpGet]
    public async Task<IActionResult> GetClasses(Guid centerId)
        => Ok(await _classService.GetClassesAsync(centerId));

    [HttpPost]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateClass(Guid centerId, [FromBody] CreateClassRequest request)
    {
        var c = await _classService.CreateClassAsync(centerId, request);
        return Created("", c);
    }

    [HttpPost("{classId:guid}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> AddStudents(Guid centerId, Guid classId, [FromBody] AddStudentsRequest request)
    {
        await _classService.AddStudentsAsync(classId, request);
        return NoContent();
    }

    [HttpGet("{classId:guid}/students")]
    public async Task<IActionResult> GetStudents(Guid centerId, Guid classId)
        => Ok(await _classService.GetClassStudentsAsync(classId));

    [HttpPost("{classId:guid}/lessons")]
    [Authorize(Roles = "CenterAdmin,Teacher")]
    public async Task<IActionResult> AssignLesson(Guid centerId, Guid classId, [FromBody] AssignLessonRequest request)
    {
        var teacherId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _classService.AssignLessonAsync(classId, teacherId, request);
        return NoContent();
    }
}
