using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Commands.UpdateProfile;
using SignMate.Application.Features.Users.Queries.GetAllUsers;
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
