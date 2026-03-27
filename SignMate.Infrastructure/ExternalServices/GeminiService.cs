using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(10);
        _apiKey = config["Gemini:ApiKey"] ?? "";
        _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
    }

    public async Task<string?> GenerateDetailedFeedbackAsync(GeminiFeedbackContext context)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Skipping AI feedback.");
            return null;
        }

        try
        {
            var prompt = BuildPrompt(context);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 300
                }
            };

            var response = await _http.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Gemini API error {StatusCode}: {Error}", response.StatusCode, error);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var text = json
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Gemini API request timed out.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini AI feedback generation failed.");
            return null;
        }
    }

    private static string BuildPrompt(GeminiFeedbackContext ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là huấn luyện viên ngôn ngữ ký hiệu Việt Nam (VSL). Hãy đưa ra phản hồi chi tiết, khích lệ và thực tế cho học viên.");
        sb.AppendLine();
        sb.AppendLine($"Ký hiệu vừa tập: \"{ctx.SignWord}\"");
        sb.AppendLine($"Điểm tổng: {ctx.OverallScore:P0}");
        sb.AppendLine($"Lần thử thứ: {ctx.AttemptNumber}");

        if (ctx.PreviousScore.HasValue)
        {
            var diff = ctx.OverallScore - ctx.PreviousScore.Value;
            sb.AppendLine($"Điểm lần trước: {ctx.PreviousScore.Value:P0} ({(diff >= 0 ? "+" : "")}{diff:P0})");
        }

        sb.AppendLine();
        sb.AppendLine("Điểm chi tiết từng chiều:");
        foreach (var fb in ctx.DtwFeedbacks)
        {
            sb.AppendLine($"- {fb.Type}: {fb.Score:P0}");
        }

        sb.AppendLine();
        sb.AppendLine("Yêu cầu:");
        sb.AppendLine("1. Nhận xét tổng quan (1-2 câu, có emoji khích lệ)");
        sb.AppendLine("2. Chỉ ra 1-2 điểm yếu nhất và gợi ý cải thiện cụ thể");
        sb.AppendLine("3. Khen ngợi điểm mạnh nhất");
        sb.AppendLine("4. Nếu có tiến bộ so với lần trước, hãy khen");
        sb.AppendLine("5. Đưa ra 1 mẹo thực hành ngắn gọn");
        sb.AppendLine();
        sb.AppendLine("Trả lời bằng tiếng Việt, ngắn gọn (tối đa 150 từ), thân thiện, không dùng markdown.");

        return sb.ToString();
    }
}
