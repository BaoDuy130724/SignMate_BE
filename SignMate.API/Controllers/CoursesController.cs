using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Features.Courses.Commands.CreateCourse;
using SignMate.Application.Features.Courses.Commands.DeleteCourse;
using SignMate.Application.Features.Courses.Commands.UpdateCourse;
using SignMate.Application.Features.Courses.Queries.GetCourseById;
using SignMate.Application.Features.Courses.Queries.GetCourses;
using SignMate.Application.Features.Courses.Queries.GetLessonsByCourse;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý khóa học và danh sách bài học con của khóa.
/// </summary>
[Route("api/courses")]
public class CoursesController : BaseApiController
{
    /// <summary>Danh sách khóa học có lọc. <c>GET /api/courses</c>.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? search, [FromQuery] string? level, [FromQuery] bool includeUnpublished = false)
    {
        var result = await Mediator.Send(new GetCoursesQuery(search, level, includeUnpublished));
        return Success(result);
    }

    /// <summary>Chi tiết khóa học kèm bài học. <c>GET /api/courses/{id}</c>.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var result = await Mediator.Send(new GetCourseByIdQuery(id));
        return Success(result);
    }

    /// <summary>Tạo khóa học mới. <c>POST /api/courses</c>. CenterAdmin tạo khóa riêng của center (CenterId tự gán theo caller).</summary>
    [Authorize(Roles = "Teacher,SuperAdmin,CenterAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new CreateCourseCommand(request, userId));
        return Created(result, "Tạo khóa học thành công.");
    }

    /// <summary>Cập nhật khóa học. <c>PUT /api/courses/{id}</c>. Handler chặn sửa khóa ngoài center của caller.</summary>
    [Authorize(Roles = "Teacher,SuperAdmin,CenterAdmin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var result = await Mediator.Send(new UpdateCourseCommand(id, request));
        return Success(result, "Cập nhật khóa học thành công.");
    }

    /// <summary>Xóa khóa học. <c>DELETE /api/courses/{id}</c>. Handler chặn xóa khóa ngoài center của caller.</summary>
    [Authorize(Roles = "SuperAdmin,CenterAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        await Mediator.Send(new DeleteCourseCommand(id));
        return Success("Xóa khóa học thành công.");
    }

    /// <summary>Danh sách bài học con của khóa. <c>GET /api/courses/{id}/lessons</c>.</summary>
    [HttpGet("{id:int}/lessons")]
    public async Task<IActionResult> GetLessons(int id)
    {
        var result = await Mediator.Send(new GetLessonsByCourseQuery(id));
        return Success(result);
    }
}
