namespace SignMate.Domain.Entities;

public class GameSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string GameType { get; set; } = null!;
    public int XpEarned { get; set; }
    public DateTime PlayedAt { get; set; }

    public User User { get; set; } = null!;
}
