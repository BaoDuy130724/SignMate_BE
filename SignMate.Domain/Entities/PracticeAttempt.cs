namespace SignMate.Domain.Entities;

public class PracticeAttempt
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string VideoClipUrl { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
    public float OverallScore { get; set; }

    // Đánh giá THẬT từ giám khảo Gemini multimodal. Null khi không bật giám khảo hoặc Gemini
    // tạm lỗi (lúc đó IsCorrect suy từ OverallScore DTW). Verdict: "dat" | "chua_dat".
    // RubricJson lưu nguyên rubric đầy đủ (verdict + 4 tiêu chí + summary) để hiển thị lại
    // theo gói mà không phải gọi lại Gemini.
    public string? JudgeVerdict { get; set; }
    public string? JudgeRubricJson { get; set; }

    public PracticeSession Session { get; set; } = null!;
    public ICollection<AIFeedback> Feedbacks { get; set; } = [];
}
