using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Center;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Center.Commands.CreateCenter;
using SignMate.Application.Features.Center.Commands.CreateCenterUser;
using SignMate.Application.Features.Center.Commands.DeleteCenter;
using SignMate.Application.Features.Center.Commands.UpdateCenter;
using SignMate.Application.Features.Center.Commands.UpdateCenterMember;
using SignMate.Application.Features.Center.Commands.DeleteCenterMember;
using SignMate.Application.Features.Center.Queries.GetCenterDashboard;
using SignMate.Application.Features.Center.Queries.GetCenterInsight;
using SignMate.Application.Features.Center.Queries.GetCenterMembers;
using SignMate.Application.Features.Center.Queries.GetCenterMember;
using SignMate.Application.Features.Center.Queries.GetCenters;
using SignMate.Domain.Entities;
using System.Security.Claims;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý trung tâm: thông tin trung tâm, dashboard giám sát, tài khoản admin/giáo viên/học viên.
/// </summary>
[Route("api/centers")]
[Authorize(Roles = "SuperAdmin,CenterAdmin")]
public class CentersController : BaseApiController
{
    /// <summary>Danh sách trung tâm. <c>GET /api/centers</c>.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCenters()
        => Success(await Mediator.Send(new GetCentersQuery()));

    /// <summary>Tạo trung tâm mới. <c>POST /api/centers</c>.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenter([FromBody] CenterDto request)
        => Created(await Mediator.Send(new CreateCenterCommand(request)), "Tạo trung tâm thành công.");

    /// <summary>Cập nhật trung tâm. <c>PUT /api/centers/{id}</c>.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateCenter(int id, [FromBody] CenterDto request)
        => Success(await Mediator.Send(new UpdateCenterCommand(id, request)), "Cập nhật trung tâm thành công.");

    /// <summary>Xóa trung tâm. <c>DELETE /api/centers/{id}</c>.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteCenter(int id)
    {
        await Mediator.Send(new DeleteCenterCommand(id));
        return Success("Xóa trung tâm thành công.");
    }

    /// <summary>Dashboard giám sát trung tâm. <c>GET /api/centers/{id}/dashboard</c>.</summary>
    [HttpGet("{id:int}/dashboard")]
    public async Task<IActionResult> GetDashboard(int id)
        => Success(await Mediator.Send(new GetCenterDashboardQuery(id)));

    /// <summary>Nhận định AI cho trung tâm (scope theo center của caller). <c>GET /api/centers/{id}/insight?forceRefresh=</c>.</summary>
    [HttpGet("{id:int}/insight")]
    public async Task<IActionResult> GetInsight(int id, [FromQuery] bool forceRefresh = false)
        => Success(await Mediator.Send(new GetCenterInsightQuery(id, forceRefresh)));

    /// <summary>Tạo tài khoản CenterAdmin. <c>POST /api/centers/{id}/admin</c>.</summary>
    [HttpPost("{id:int}/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenterAdmin(int id, [FromBody] CreateCenterAdminRequest request)
    {
        await Mediator.Send(new CreateCenterUserCommand(id, UserRole.CenterAdmin, request));
        return Success("Tạo quản trị viên trung tâm thành công.");
    }

    /// <summary>Chi tiết thành viên của trung tâm. <c>GET /api/centers/{id}/members/{userId}</c>.</summary>
    [HttpGet("{id:int}/members/{userId:int}")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetCenterMember(int id, int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetCenterMemberQuery(id, userId, currentUserId));
        return Success(result);
    }

    /// <summary>Cập nhật thành viên của trung tâm. <c>PUT /api/centers/{id}/members/{userId}</c>.</summary>
    [HttpPut("{id:int}/members/{userId:int}")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> UpdateCenterMember(int id, int userId, [FromBody] UpdateCenterMemberRequest request)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new UpdateCenterMemberCommand(id, userId, currentUserId, request));
        return Success(result, "Cập nhật thành viên thành công.");
    }

    /// <summary>Gỡ thành viên khỏi trung tâm. <c>DELETE /api/centers/{id}/members/{userId}</c>.</summary>
    [HttpDelete("{id:int}/members/{userId:int}")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> DeleteCenterMember(int id, int userId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new DeleteCenterMemberCommand(id, userId, currentUserId));
        return Success("Xóa thành viên thành công.");
    }

    /// <summary>Danh sách giáo viên của trung tâm. <c>GET /api/centers/{id}/teachers</c>.</summary>
    [HttpGet("{id:int}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetTeachers(int id)
        => Success(await Mediator.Send(new GetCenterMembersQuery(id, UserRole.Teacher)));

    /// <summary>Tạo tài khoản giáo viên. <c>POST /api/centers/{id}/teachers</c>.</summary>
    [HttpPost("{id:int}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateTeacher(int id, [FromBody] CreateCenterAdminRequest request)
    {
        await Mediator.Send(new CreateCenterUserCommand(id, UserRole.Teacher, request));
        return Success("Tạo giáo viên thành công.");
    }

    /// <summary>Danh sách học viên của trung tâm. <c>GET /api/centers/{id}/students</c>.</summary>
    [HttpGet("{id:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetStudents(int id)
        => Success(await Mediator.Send(new GetCenterMembersQuery(id, UserRole.Student)));

    /// <summary>Tạo tài khoản học viên. <c>POST /api/centers/{id}/students</c>.</summary>
    [HttpPost("{id:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateStudent(int id, [FromBody] CreateCenterAdminRequest request)
    {
        await Mediator.Send(new CreateCenterUserCommand(id, UserRole.Student, request));
        return Success("Tạo học viên thành công.");
    }
}
