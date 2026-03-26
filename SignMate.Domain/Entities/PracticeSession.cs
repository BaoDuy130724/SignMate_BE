namespace SignMate.Domain.Entities;

public class PracticeSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SignId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalAttempts { get; set; }

    public User User { get; set; } = null!;
    public Sign Sign { get; set; } = null!;
    public ICollection<PracticeAttempt> Attempts { get; set; } = [];
}
