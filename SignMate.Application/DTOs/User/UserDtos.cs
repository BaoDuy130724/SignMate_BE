using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.User;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
}

public class StreakDto
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly LastActiveDate { get; set; }
}

public class AchievementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? IconUrl { get; set; }
    public DateTime EarnedAt { get; set; }
}
