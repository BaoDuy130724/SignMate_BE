using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.User;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = null!;
    public string Plan { get; set; } = "Free";
    public int? CenterId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Streak { get; set; }
    public int TotalXp { get; set; }
    public int Level { get; set; } = 1;
    public int LessonsCompleted { get; set; }
    public int PracticeAccuracy { get; set; }
}

public class UpdateProfileRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
}

/// <summary>Tạo người dùng mới do SuperAdmin thực hiện (không qua OTP).</summary>
public class CreateUserRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    [Required, MinLength(6)]
    public string Password { get; set; } = null!;
    [Required, MaxLength(200)]
    public string FullName { get; set; } = null!;
    /// <summary>Tên vai trò: Student | Teacher | CenterAdmin | SuperAdmin.</summary>
    public string Role { get; set; } = "Student";
    public int? CenterId { get; set; }
}

/// <summary>Cập nhật người dùng (partial). Trường null sẽ được bỏ qua.</summary>
public class UpdateUserRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    /// <summary>Tên vai trò mới (tùy chọn).</summary>
    public string? Role { get; set; }
    /// <summary>Trung tâm trực thuộc; gửi 0 để gỡ khỏi trung tâm.</summary>
    public int? CenterId { get; set; }
}

public class StreakDto
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly LastActiveDate { get; set; }
}

public class AchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? IconUrl { get; set; }
    public DateTime EarnedAt { get; set; }
}
