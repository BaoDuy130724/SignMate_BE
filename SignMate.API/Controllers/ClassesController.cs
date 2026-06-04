using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Features.Classes.Commands.AddStudents;
using SignMate.Application.Features.Classes.Commands.AssignLesson;
using SignMate.Application.Features.Classes.Commands.CreateClass;
using SignMate.Application.Features.Classes.Queries.GetClasses;
using SignMate.Application.Features.Classes.Queries.GetClassStudents;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý lớp học trong một trung tâm: danh sách lớp, học viên, giao bài.
/// </summary>
[Route("api/centers/{centerId:int}/classes")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class ClassesController : BaseApiController
{
    /// <summary>Danh sách lớp của trung tâm. <c>GET /api/centers/{centerId}/classes</c>.</summary>
    [HttpGet]
    public async Task<IActionResult> GetClasses(int centerId)
        => Success(await Mediator.Send(new GetClassesQuery(centerId)));

    /// <summary>Tạo lớp mới. <c>POST /api/centers/{centerId}/classes</c>.</summary>
    [HttpPost]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateClass(int centerId, [FromBody] CreateClassRequest request)
        => Created(await Mediator.Send(new CreateClassCommand(centerId, request)), "Tạo lớp thành công.");

    /// <summary>Thêm học viên vào lớp. <c>POST /api/centers/{centerId}/classes/{classId}/students</c>.</summary>
    [HttpPost("{classId:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> AddStudents(int centerId, int classId, [FromBody] AddStudentsRequest request)
    {
        await Mediator.Send(new AddStudentsCommand(classId, request));
        return Success("Thêm học viên vào lớp thành công.");
    }

    /// <summary>Danh sách học viên trong lớp. <c>GET /api/centers/{centerId}/classes/{classId}/students</c>.</summary>
    [HttpGet("{classId:int}/students")]
    public async Task<IActionResult> GetStudents(int centerId, int classId)
        => Success(await Mediator.Send(new GetClassStudentsQuery(classId)));

    /// <summary>Giao bài cho lớp. <c>POST /api/centers/{centerId}/classes/{classId}/lessons</c>.</summary>
    [HttpPost("{classId:int}/lessons")]
    [Authorize(Roles = "CenterAdmin,Teacher")]
    public async Task<IActionResult> AssignLesson(int centerId, int classId, [FromBody] AssignLessonRequest request)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new AssignLessonCommand(classId, teacherId, request));
        return Success("Giao bài học cho lớp thành công.");
    }
}
