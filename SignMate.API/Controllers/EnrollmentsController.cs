using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Enrollment;
using SignMate.Application.Features.Enrollments.Commands.Enroll;
using SignMate.Application.Features.Enrollments.Queries.GetMyEnrollments;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý việc đăng ký khóa học của học viên.
/// </summary>
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : BaseApiController
{
    /// <summary>
    /// Đăng ký học viên hiện tại vào một khóa học. <c>POST /api/enrollments</c>.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new EnrollCommand(userId, request));
        return Created(result, "Đăng ký khóa học thành công.");
    }

    /// <summary>
    /// Danh sách khóa học đã đăng ký kèm tiến độ của học viên hiện tại. <c>GET /api/enrollments/me</c>.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMyEnrollmentsQuery(userId));
        return Success(result);
    }
}
