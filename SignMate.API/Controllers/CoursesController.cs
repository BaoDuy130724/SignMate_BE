using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService) => _courseService = courseService;

    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] string? search, [FromQuery] string? level)
        => Ok(await _courseService.GetCoursesAsync(search, level));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var course = await _courseService.CreateCourseAsync(request, userId);
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _courseService.UpdateCourseAsync(id, request);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpGet("{id:guid}/lessons")]
    public async Task<IActionResult> GetLessons(Guid id)
        => Ok(await _courseService.GetLessonsByCourseAsync(id));
}
