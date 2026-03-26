using Microsoft.AspNetCore.Mvc;
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
}
