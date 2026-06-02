namespace SignMate.Domain.Entities;

public class SignProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SignId { get; set; }
    public bool IsMastered { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? LastPracticedAt { get; set; }

    public User User { get; set; } = null!;
    public Sign Sign { get; set; } = null!;
}
