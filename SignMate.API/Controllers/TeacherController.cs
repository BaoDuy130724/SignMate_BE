using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Features.Teacher.Commands.AddComment;
using SignMate.Application.Features.Teacher.Commands.UpdateComment;
using SignMate.Application.Features.Teacher.Commands.DeleteComment;
using SignMate.Application.Features.Teacher.Queries.GetClasses;
using SignMate.Application.Features.Teacher.Queries.GetDashboard;
using SignMate.Application.Features.Teacher.Queries.GetStudentComments;
using SignMate.Application.Features.Teacher.Queries.GetStudents;

namespace SignMate.API.Controllers;

/// <summary>
/// Tính năng hỗ trợ giảng dạy cho giáo viên: dashboard, lớp học, học sinh, nhận xét.
/// </summary>
[Route("api/teacher")]
[Authorize(Roles = "CenterAdmin,Teacher")]
public class TeacherController : BaseApiController
{
    /// <summary>Gửi nhận xét cho học viên. <c>POST /api/teacher/comments</c>.</summary>
    [HttpPost("comments")]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new AddCommentCommand(teacherId, request));
        return Created(result, "Gửi nhận xét thành công.");
    }

    /// <summary>Cập nhật nhận xét. <c>PUT /api/teacher/comments/{id}</c>.</summary>
    [HttpPut("comments/{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new UpdateCommentCommand(teacherId, id, request));
        return Success(result, "Cập nhật nhận xét thành công.");
    }

    /// <summary>Xóa nhận xét. <c>DELETE /api/teacher/comments/{id}</c>.</summary>
    [HttpDelete("comments/{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new DeleteCommentCommand(teacherId, id));
        return Success("Xóa nhận xét thành công.");
    }

    /// <summary>Nhận xét đã gửi cho một học viên. <c>GET /api/teacher/students/{studentId}/comments</c>.</summary>
    [HttpGet("students/{studentId:int}/comments")]
    public async Task<IActionResult> GetStudentComments(int studentId)
    {
        var result = await Mediator.Send(new GetStudentCommentsQuery(studentId));
        return Success(result);
    }

    /// <summary>Số liệu tổng quan của giáo viên. <c>GET /api/teacher/dashboard</c>.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetTeacherDashboardQuery(teacherId));
        return Success(result);
    }

    /// <summary>Danh sách lớp phụ trách. <c>GET /api/teacher/classes</c>.</summary>
    [HttpGet("classes")]
    public async Task<IActionResult> GetClasses()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetTeacherClassesQuery(teacherId));
        return Success(result);
    }

    /// <summary>Danh sách học viên quản lý. <c>GET /api/teacher/students</c>.</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var teacherId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetTeacherStudentsQuery(teacherId));
        return Success(result);
    }
}
