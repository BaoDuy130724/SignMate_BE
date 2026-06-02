using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/centers/{centerId:int}/classes")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
        => _classService = classService;

    [HttpGet]
    public async Task<IActionResult> GetClasses(int centerId)
        => Ok(await _classService.GetClassesAsync(centerId));

    [HttpPost]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateClass(int centerId, [FromBody] CreateClassRequest request)
    {
        var c = await _classService.CreateClassAsync(centerId, request);
        return Created("", c);
    }

    [HttpPost("{classId:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> AddStudents(int centerId, int classId, [FromBody] AddStudentsRequest request)
    {
        await _classService.AddStudentsAsync(classId, request);
        return NoContent();
    }

    [HttpGet("{classId:int}/students")]
    public async Task<IActionResult> GetStudents(int centerId, int classId)
        => Ok(await _classService.GetClassStudentsAsync(classId));

    [HttpPost("{classId:int}/lessons")]
    [Authorize(Roles = "CenterAdmin,Teacher")]
    public async Task<IActionResult> AssignLesson(int centerId, int classId, [FromBody] AssignLessonRequest request)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _classService.AssignLessonAsync(classId, teacherId, request);
        return NoContent();
    }
}
