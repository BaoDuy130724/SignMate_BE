namespace SignMate.Domain.Entities;

public class AIFeedback
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public FeedbackType FeedbackType { get; set; }
    public float Score { get; set; }
    public string Message { get; set; } = null!;
    public string? KeypointData { get; set; }
    public DateTime CreatedAt { get; set; }

    public PracticeAttempt Attempt { get; set; } = null!;
}

public enum FeedbackType { HandShape, Movement, Location, PalmOrientation }
