using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Features.Lessons.Commands.CreateLesson;
using SignMate.Application.Features.Lessons.Commands.DeleteLesson;
using SignMate.Application.Features.Lessons.Commands.UpdateLesson;
using SignMate.Application.Features.Lessons.Queries.GetLessonById;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý bài học con và nội dung chi tiết (kèm từ vựng).
/// </summary>
[Route("api/lessons")]
public class LessonsController : BaseApiController
{
    /// <summary>Chi tiết bài học kèm từ vựng. <c>GET /api/lessons/{id}</c>.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var result = await Mediator.Send(new GetLessonByIdQuery(id));
        return Success(result);
    }

    /// <summary>Tạo bài học trong khóa. <c>POST /api/lessons/course/{courseId}</c>.</summary>
    [Authorize(Roles = "SuperAdmin,CenterAdmin")]
    [HttpPost("course/{courseId:int}")]
    public async Task<IActionResult> CreateLesson(int courseId, [FromBody] CreateLessonRequest request)
    {
        var result = await Mediator.Send(new CreateLessonCommand(courseId, request));
        return Created(result, "Tạo bài học thành công.");
    }

    /// <summary>Cập nhật bài học. <c>PUT /api/lessons/{id}</c>.</summary>
    [Authorize(Roles = "SuperAdmin,CenterAdmin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonRequest request)
    {
        var result = await Mediator.Send(new UpdateLessonCommand(id, request));
        return Success(result, "Cập nhật bài học thành công.");
    }

    /// <summary>Xóa bài học. <c>DELETE /api/lessons/{id}</c>.</summary>
    [Authorize(Roles = "SuperAdmin,CenterAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        await Mediator.Send(new DeleteLessonCommand(id));
        return Success("Xóa bài học thành công.");
    }
}
