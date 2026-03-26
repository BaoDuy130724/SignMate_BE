namespace SignMate.Domain.Entities;

public class SignProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SignId { get; set; }
    public bool IsMastered { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastPracticedAt { get; set; }

    public User User { get; set; } = null!;
    public Sign Sign { get; set; } = null!;
}
