namespace SignMate.Domain.Entities;

public class UserAchievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime EarnedAt { get; set; }

    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
