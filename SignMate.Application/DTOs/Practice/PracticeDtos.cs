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
