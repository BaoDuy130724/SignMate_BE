namespace SignMate.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Goal { get; set; }
    public string? Level { get; set; }
    public bool IsOnboarded { get; set; }
    public int XpPoints { get; set; }
    public Guid? CenterId { get; set; }
    public Center? Center { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<PracticeSession> PracticeSessions { get; set; } = [];
    public Streak? Streak { get; set; }
}

public enum UserRole { Student, Teacher, CenterAdmin, SuperAdmin }
