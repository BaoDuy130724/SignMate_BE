namespace SignMate.Domain.Entities;

public class PracticeSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SignId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalAttempts { get; set; }

    public User User { get; set; } = null!;
    public Sign Sign { get; set; } = null!;
    public ICollection<PracticeAttempt> Attempts { get; set; } = [];
}
