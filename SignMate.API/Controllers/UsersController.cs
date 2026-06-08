using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Commands.CreateUser;
using SignMate.Application.Features.Users.Commands.DeleteUser;
using SignMate.Application.Features.Users.Commands.UpdateProfile;
using SignMate.Application.Features.Users.Commands.UpdateUser;
using SignMate.Application.Features.Users.Queries.GetAllUsers;
using SignMate.Application.Features.Users.Queries.GetUserById;
using SignMate.Application.Features.Users.Queries.GetMyAchievements;
using SignMate.Application.Features.Users.Queries.GetMyProfile;
using SignMate.Application.Features.Users.Queries.GetMyStreak;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý hồ sơ người dùng và các chỉ số học tập liên quan (streak, thành tích).
/// </summary>
[Route("api/users")]
[Authorize]
public class UsersController : BaseApiController
{
    /// <summary>
    /// Danh sách người dùng cho quản trị viên, lọc theo vai trò. <c>GET /api/users</c>.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role)
    {
        var result = await Mediator.Send(new GetAllUsersQuery(role));
        return Success(result);
    }

    /// <summary>
    /// Chi tiết một người dùng cho quản trị viên. <c>GET /api/users/{id}</c>.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id));
        return Success(result);
    }

    /// <summary>
    /// Tạo người dùng mới (SuperAdmin, không qua OTP). <c>POST /api/users</c>.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await Mediator.Send(new CreateUserCommand(request));
        return Created(result, "Tạo người dùng thành công.");
    }

    /// <summary>
    /// Cập nhật người dùng bất kỳ. <c>PUT /api/users/{id}</c>.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await Mediator.Send(new UpdateUserCommand(id, request));
        return Success(result, "Cập nhật người dùng thành công.");
    }

    /// <summary>
    /// Xóa người dùng. <c>DELETE /api/users/{id}</c>.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new DeleteUserCommand(id, currentUserId));
        return Success("Xóa người dùng thành công.");
    }

    /// <summary>
    /// Hồ sơ đầy đủ của người dùng hiện tại. <c>GET /api/users/me</c>.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMyProfileQuery(userId));
        return Success(result);
    }

    /// <summary>
    /// Cập nhật hồ sơ người dùng hiện tại. <c>PUT /api/users/me</c>.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new UpdateProfileCommand(userId, request));
        return Success(result, "Cập nhật hồ sơ thành công.");
    }

    /// <summary>
    /// Thông số streak của người dùng hiện tại. <c>GET /api/users/me/streak</c>.
    /// </summary>
    [HttpGet("me/streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMyStreakQuery(userId));
        return Success(result);
    }

    /// <summary>
    /// Danh sách thành tích đã đạt của người dùng hiện tại. <c>GET /api/users/me/achievements</c>.
    /// </summary>
    [HttpGet("me/achievements")]
    public async Task<IActionResult> GetAchievements()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMyAchievementsQuery(userId));
        return Success(result);
    }
}
