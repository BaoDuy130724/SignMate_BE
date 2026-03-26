namespace SignMate.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? IconUrl { get; set; }
    public string ConditionType { get; set; } = null!;
    public int ConditionValue { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}
