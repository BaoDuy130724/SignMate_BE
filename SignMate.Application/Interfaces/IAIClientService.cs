namespace SignMate.Application.Interfaces;

public interface IAIClientService
{
    Task<AIAnalysisResult> AnalyzeAsync(string videoUrl, string signId, string? referenceKeypoints);
}

public class AIAnalysisResult
{
    public float OverallScore { get; set; }
    public List<FeedbackItem> Feedbacks { get; set; } = [];
    public string KeypointData { get; set; } = null!;
}

public class FeedbackItem
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}
