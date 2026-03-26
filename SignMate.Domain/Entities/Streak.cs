namespace SignMate.Domain.Entities;

public class Streak
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly LastActiveDate { get; set; }

    public User User { get; set; } = null!;
}
