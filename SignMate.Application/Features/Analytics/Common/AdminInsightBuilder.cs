using System.Text.Json;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Analytics.Common;

/// <summary>
/// Gọi Gemini với một prompt phân tích rồi parse kết quả thành <see cref="AdminInsightDto"/>.
/// Dùng chung cho insight phân tích & doanh thu. Mọi nhánh đều degrade êm:
/// - Gemini trả null (chưa có key / lỗi) → <c>AiAvailable=false</c>.
/// - Trả text không phải JSON hợp lệ → nhét nguyên text vào <c>Summary</c> (vẫn hữu ích).
/// </summary>
public static class AdminInsightBuilder
{
    private const string ModelName = "gemini-2.5-flash";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Phần khung prompt yêu cầu Gemini trả JSON đúng định dạng (ghép trước nội dung số liệu).</summary>
    public static string JsonInstruction =>
        "Chỉ trả về DUY NHẤT một đối tượng JSON hợp lệ (không kèm giải thích, không markdown, không ```), " +
        "theo đúng schema:\n" +
        "{\n" +
        "  \"summary\": \"1-2 câu nhận định tổng quan, có thể kèm emoji\",\n" +
        "  \"positives\": [\"điểm tích cực 1\", \"...\"],\n" +
        "  \"concerns\": [\"điểm đáng lưu ý 1\", \"...\"],\n" +
        "  \"recommendations\": [\"khuyến nghị hành động 1\", \"...\"]\n" +
        "}\n" +
        "Mỗi mảng 2-4 mục, mỗi mục ngắn gọn (tối đa ~20 từ), tiếng Việt, dựa HOÀN TOÀN vào số liệu đã cho, " +
        "tuyệt đối không bịa thêm con số không có trong dữ liệu.";

    public static async Task<AdminInsightDto> BuildAsync(IGeminiService gemini, string prompt)
    {
        var raw = await gemini.GenerateAsync(prompt, maxOutputTokens: 700);

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new AdminInsightDto
            {
                AiAvailable = false,
                Summary = "Tính năng AI chưa được cấu hình hoặc tạm thời không khả dụng.",
                GeneratedAt = DateTime.UtcNow,
                Model = ModelName
            };
        }

        var dto = TryParse(raw);
        dto.AiAvailable = true;
        dto.GeneratedAt = DateTime.UtcNow;
        dto.Model = ModelName;
        return dto;
    }

    private static AdminInsightDto TryParse(string raw)
    {
        var json = ExtractJsonObject(raw);
        if (json != null)
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<InsightPayload>(json, JsonOpts);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Summary))
                {
                    return new AdminInsightDto
                    {
                        Summary = parsed.Summary!.Trim(),
                        Positives = Clean(parsed.Positives),
                        Concerns = Clean(parsed.Concerns),
                        Recommendations = Clean(parsed.Recommendations)
                    };
                }
            }
            catch (JsonException)
            {
                // rơi xuống fallback bên dưới
            }
        }

        // Fallback: không parse được JSON → vẫn hiển thị text thô như tóm tắt.
        return new AdminInsightDto { Summary = raw.Trim() };
    }

    /// <summary>Lấy chuỗi từ dấu '{' đầu tiên tới '}' cuối cùng (bỏ ```json fence / chữ thừa quanh JSON).</summary>
    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start) return text.Substring(start, end - start + 1);
        return null;
    }

    private static List<string> Clean(List<string>? items)
        => items?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList() ?? [];

    private sealed class InsightPayload
    {
        public string? Summary { get; set; }
        public List<string>? Positives { get; set; }
        public List<string>? Concerns { get; set; }
        public List<string>? Recommendations { get; set; }
    }
}
