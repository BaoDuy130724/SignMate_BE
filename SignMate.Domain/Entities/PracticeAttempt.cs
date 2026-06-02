namespace SignMate.Domain.Entities;

public class PracticeAttempt
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string VideoClipUrl { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
    public float OverallScore { get; set; }

    public PracticeSession Session { get; set; } = null!;
    public ICollection<AIFeedback> Feedbacks { get; set; } = [];
}
