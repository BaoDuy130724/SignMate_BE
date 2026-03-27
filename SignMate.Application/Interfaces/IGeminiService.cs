namespace SignMate.Application.Interfaces;

public interface IGeminiService
{
    Task<string?> GenerateDetailedFeedbackAsync(GeminiFeedbackContext context);
}

public class GeminiFeedbackContext
{
    public string SignWord { get; set; } = null!;
    public float OverallScore { get; set; }
    public List<FeedbackItem> DtwFeedbacks { get; set; } = [];
    public int AttemptNumber { get; set; }
    public float? PreviousScore { get; set; }
}
