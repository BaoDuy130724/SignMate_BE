using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class TeacherController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeacherController(ITeacherService teacherService)
        => _teacherService = teacherService;

    [HttpPost("comments")]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        var teacherId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var comment = await _teacherService.AddCommentAsync(teacherId, request);
        return Created("", comment);
    }

    [HttpGet("students/{studentId:guid}/comments")]
    public async Task<IActionResult> GetStudentComments(Guid studentId)
        => Ok(await _teacherService.GetStudentCommentsAsync(studentId));

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var teacherId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _teacherService.GetTeacherDashboardAsync(teacherId));
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var teacherId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _teacherService.GetTeacherClassesAsync(teacherId));
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var teacherId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _teacherService.GetTeacherStudentsAsync(teacherId));
    }
}
