using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ICourseService _courseService;

    public LessonsController(ICourseService courseService) => _courseService = courseService;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLesson(Guid id)
    {
        var lesson = await _courseService.GetLessonByIdAsync(id);
        return lesson == null ? NotFound() : Ok(lesson);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("course/{courseId:guid}")]
    public async Task<IActionResult> CreateLesson(Guid courseId, [FromBody] CreateLessonRequest request)
        => Ok(await _courseService.CreateLessonAsync(courseId, request));

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] UpdateLessonRequest request)
    {
        var lesson = await _courseService.UpdateLessonAsync(id, request);
        return lesson == null ? NotFound() : Ok(lesson);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        await _courseService.DeleteLessonAsync(id);
        return NoContent();
    }
}
