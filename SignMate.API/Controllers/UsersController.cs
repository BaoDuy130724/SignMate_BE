using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISignMateDbContext _db;
    private readonly IStreakService _streakService;

    public UsersController(ISignMateDbContext db, IStreakService streakService)
    {
        _db = db;
        _streakService = streakService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new UserProfileDto
        {
            Id = user.Id, Email = user.Email, FullName = user.FullName,
            AvatarUrl = user.AvatarUrl, Role = user.Role.ToString(), CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UserProfileDto
        {
            Id = user.Id, Email = user.Email, FullName = user.FullName,
            AvatarUrl = user.AvatarUrl, Role = user.Role.ToString(), CreatedAt = user.CreatedAt
        });
    }

    [HttpGet("me/streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var streak = await _streakService.GetStreakAsync(userId);

        return Ok(streak == null
            ? new StreakDto { CurrentStreak = 0, LongestStreak = 0, LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow) }
            : new StreakDto { CurrentStreak = streak.CurrentStreak, LongestStreak = streak.LongestStreak, LastActiveDate = streak.LastActiveDate });
    }

    [HttpGet("me/achievements")]
    public async Task<IActionResult> GetAchievements()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var achievements = await _db.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new AchievementDto
            {
                Id = ua.Achievement.Id, Name = ua.Achievement.Name,
                Description = ua.Achievement.Description, IconUrl = ua.Achievement.IconUrl,
                EarnedAt = ua.EarnedAt
            })
            .ToListAsync();

        return Ok(achievements);
    }
}
