using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Practice;

public class StartSessionRequest
{
    [Required]
    public int SignId { get; set; }
}

public class StartSessionResponse
{
    public int SessionId { get; set; }
}

public class EndSessionRequest
{
    [Required]
    public int SessionId { get; set; }
}

public class AttemptResponse
{
    public int AttemptId { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
    // Nhận xét chi tiết Gemini KHÔNG còn trả tại đây — chấm điểm trả về ngay để app hiện
    // tức thì; app lấy nhận xét qua GET /api/practice/attempt/{id}/feedback (xem
    // AttemptFeedbackResponse). Giữ field cho client cũ; luôn null ở luồng mới.
    public string? GeminiFeedback { get; set; }

    // === Đánh giá THẬT (giám khảo Gemini multimodal), phân tầng theo gói ===
    // IsCorrect là nguồn sự thật mới cho "Đạt/Chưa đạt": lấy từ verdict giám khảo khi có,
    // fallback OverallScore >= 0.7 (DTW) khi giám khảo không chạy được.
    public bool IsCorrect { get; set; }
    public string? Verdict { get; set; }     // "dat" | "chua_dat" | null (không có giám khảo)
    public string? Summary { get; set; }     // 1 câu nhận xét — mọi gói đều thấy
    public JudgeRubricDto? Rubric { get; set; }  // chi tiết tiêu chí — Basic (tiêu chí yếu) / Pro-B2B (đầy đủ); Free = null
}

/// <summary>Rubric đánh giá theo tiêu chí, đã trim theo gói. Tiêu chí rỗng = gói không được xem chi tiết.</summary>
public class JudgeRubricDto
{
    public string Verdict { get; set; } = "chua_dat";
    public float Confidence { get; set; }
    public string Summary { get; set; } = "";
    public List<JudgeCriterionDto> Criteria { get; set; } = [];
}

public class JudgeCriterionDto
{
    public string Key { get; set; } = "";     // hand_shape | location | movement | palm_orientation
    public string Label { get; set; } = "";   // nhãn tiếng Việt hiển thị
    public string Status { get; set; } = "";  // good | needs_work | wrong
    public string Note { get; set; } = "";
}

/// <summary>
/// Nhận xét chi tiết do Gemini sinh cho một lượt thử — lấy tách khỏi luồng chấm điểm để
/// điểm số hiện ra ngay. Chỉ có giá trị với gói Pro/B2B; null nếu không thuộc gói (hoặc Gemini
/// tạm lỗi). Phân tầng quyền áp ở handler, giống lúc còn nằm trong SubmitAttempt.
/// </summary>
public class AttemptFeedbackResponse
{
    public string? Feedback { get; set; }
}

public class FeedbackDto
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}

public class PracticeHistoryDto
{
    public int SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalAttempts { get; set; }
    public List<AttemptSummaryDto> Attempts { get; set; } = [];
}

public class AttemptSummaryDto
{
    public int Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
}
