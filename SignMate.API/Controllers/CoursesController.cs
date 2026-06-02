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
    public async Task<IActionResult> GetCourses([FromQuery] string? search, [FromQuery] string? level, [FromQuery] bool includeUnpublished = false)
        => Ok(await _courseService.GetCoursesAsync(search, level, includeUnpublished));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [Authorize(Roles = "Teacher,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var course = await _courseService.CreateCourseAsync(request, userId);
        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
    }

    [Authorize(Roles = "Teacher,SuperAdmin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _courseService.UpdateCourseAsync(id, request);
        return course == null ? NotFound() : Ok(course);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        await _courseService.DeleteCourseAsync(id);
        return NoContent();
    }

    [HttpGet("{id:int}/lessons")]
    public async Task<IActionResult> GetLessons(int id)
        => Ok(await _courseService.GetLessonsByCourseAsync(id));
}
