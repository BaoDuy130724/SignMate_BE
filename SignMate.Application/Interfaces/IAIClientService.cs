using System.Text.Json.Serialization;

namespace SignMate.Application.Interfaces;

public interface IAIClientService
{
    Task<AIAnalysisResult> AnalyzeAsync(
        string videoUrl, string signId, string? referenceKeypoints,
        string? referenceVideoUrl = null, string? signWord = null);
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

    // Đánh giá THẬT từ giám khảo Gemini multimodal (null nếu không bật / Gemini lỗi → degrade về điểm DTW).
    public JudgeResult? Judge { get; set; }
}

public class FeedbackItem
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}

/// <summary>Rubric đánh giá thật do giám khảo Gemini multimodal trả về (snake_case từ Python).</summary>
public class JudgeResult
{
    public string Verdict { get; set; } = "chua_dat";  // dat | chua_dat
    public float Confidence { get; set; }

    [JsonPropertyName("hand_shape")]
    public JudgeCriterion HandShape { get; set; } = new();
    public JudgeCriterion Location { get; set; } = new();
    public JudgeCriterion Movement { get; set; } = new();

    [JsonPropertyName("palm_orientation")]
    public JudgeCriterion PalmOrientation { get; set; } = new();

    public string Summary { get; set; } = "";
}

public class JudgeCriterion
{
    public string Status { get; set; } = "needs_work";  // good | needs_work | wrong
    public string Note { get; set; } = "";
}
