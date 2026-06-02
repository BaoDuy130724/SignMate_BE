using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStreakService _streakService;

    public UsersController(IUnitOfWork unitOfWork, IStreakService streakService)
    {
        _unitOfWork = unitOfWork;
        _streakService = streakService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role)
    {
        var query = _unitOfWork.Repository<User>().Query();
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProfileDto
            {
                Id = u.Id, Email = u.Email, FullName = u.FullName,
                AvatarUrl = u.AvatarUrl, Role = u.Role.ToString(),
                CenterId = u.CenterId, CreatedAt = u.CreatedAt
            }).ToListAsync();
        return Ok(users);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) return NotFound();

        var activeSub = await _unitOfWork.Repository<UserSubscription>().Query()
            .Include(us => us.Plan)
            .Where(us => us.UserId == userId && us.IsActive && us.EndDate >= DateTime.UtcNow)
            .OrderByDescending(us => us.EndDate)
            .FirstOrDefaultAsync();
            
        var currentPlan = activeSub != null ? activeSub.Plan.Type.ToString() : "Free";

        // Fetch additional stats that Mobile expects
        var streak = await _streakService.GetStreakAsync(userId);

        var completedLessons = await _unitOfWork.Repository<LessonProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed);

        double avgAcc = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;

        int level = (user.XpPoints / 500) + 1;

        return Ok(new UserProfileDto
        {
            Id = user.Id, Email = user.Email, FullName = user.FullName,
            AvatarUrl = user.AvatarUrl, Role = user.Role.ToString(), 
            Plan = currentPlan, CenterId = user.CenterId, CreatedAt = user.CreatedAt,
            Streak = streak?.CurrentStreak ?? 0,
            TotalXp = user.XpPoints,
            Level = level,
            LessonsCompleted = completedLessons,
            PracticeAccuracy = (int)Math.Round(avgAcc * 100)
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        if (user == null) return NotFound();

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var activeSub = await _unitOfWork.Repository<UserSubscription>().Query()
            .Include(us => us.Plan)
            .Where(us => us.UserId == userId && us.IsActive && us.EndDate >= DateTime.UtcNow)
            .OrderByDescending(us => us.EndDate)
            .FirstOrDefaultAsync();
            
        var currentPlan = activeSub != null ? activeSub.Plan.Type.ToString() : "Free";

        var streak = await _streakService.GetStreakAsync(userId);
        var completedLessons = await _unitOfWork.Repository<LessonProgress>().Query()
            .CountAsync(p => p.UserId == userId && p.Status == LessonStatus.Completed);
        double avgAcc = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == userId)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync() ?? 0;
        int level = (user.XpPoints / 500) + 1;

        return Ok(new UserProfileDto
        {
            Id = user.Id, Email = user.Email, FullName = user.FullName,
            AvatarUrl = user.AvatarUrl, Role = user.Role.ToString(), 
            Plan = currentPlan, CenterId = user.CenterId, CreatedAt = user.CreatedAt,
            Streak = streak?.CurrentStreak ?? 0,
            TotalXp = user.XpPoints,
            Level = level,
            LessonsCompleted = completedLessons,
            PracticeAccuracy = (int)Math.Round(avgAcc * 100)
        });
    }

    [HttpGet("me/streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var streak = await _streakService.GetStreakAsync(userId);

        return Ok(streak == null
            ? new StreakDto { CurrentStreak = 0, LongestStreak = 0, LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow) }
            : new StreakDto { CurrentStreak = streak.CurrentStreak, LongestStreak = streak.LongestStreak, LastActiveDate = streak.LastActiveDate });
    }

    [HttpGet("me/achievements")]
    public async Task<IActionResult> GetAchievements()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var achievements = await _unitOfWork.Repository<UserAchievement>().Query()
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
