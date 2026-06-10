using System.Text.Json.Serialization;

namespace SignMate.Application.Interfaces;

public interface IAIClientService
{
    Task<AIAnalysisResult> AnalyzeAsync(string videoUrl, string signId, string? referenceKeypoints);
    Task<ExtractReferenceResult> ExtractReferenceKeypointsAsync(string videoUrl);
}

// JsonPropertyName bắt buộc: service Python trả JSON snake_case (overall_score,
// keypoint_data, total_frames, reference_keypoints) — deserializer mặc định map
// camelCase nên các field này âm thầm về 0/null (điểm lưu DB luôn = 0).
public class ExtractReferenceResult
{
    public string Status { get; set; } = null!;

    [JsonPropertyName("total_frames")]
    public int TotalFrames { get; set; }

    [JsonPropertyName("reference_keypoints")]
    public string ReferenceKeypoints { get; set; } = null!;
}

public class AIAnalysisResult
{
    [JsonPropertyName("overall_score")]
    public float OverallScore { get; set; }

    public List<FeedbackItem> Feedbacks { get; set; } = [];

    [JsonPropertyName("keypoint_data")]
    public string KeypointData { get; set; } = null!;
}

public class FeedbackItem
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}
