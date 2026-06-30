namespace SignMate.Application.Interfaces;

public interface IGeminiService
{
    Task<string?> GenerateDetailedFeedbackAsync(GeminiFeedbackContext context);

    /// <summary>
    /// Sinh văn bản tự do từ một prompt bất kỳ (dùng cho AI insight quản trị: phân tích, báo cáo,
    /// cảnh báo). Trả <c>null</c> nếu chưa cấu hình key hoặc gọi lỗi (caller tự degrade).
    /// </summary>
    Task<string?> GenerateAsync(string prompt, int maxOutputTokens = 600);
}

public class GeminiFeedbackContext
{
    public string SignWord { get; set; } = null!;
    public float OverallScore { get; set; }
    public List<FeedbackItem> DtwFeedbacks { get; set; } = [];
    public int AttemptNumber { get; set; }
    public float? PreviousScore { get; set; }
}
