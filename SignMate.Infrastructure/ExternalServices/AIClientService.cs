using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class AIClientService : IAIClientService
{
    private readonly HttpClient _http;
    private readonly string _aiBaseUrl;

    public AIClientService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _aiBaseUrl = config["AIService:BaseUrl"] ?? "http://localhost:8000";
    }

    public async Task<AIAnalysisResult> AnalyzeAsync(string videoUrl, string signId, string? referenceKeypoints)
    {
        var payload = new
        {
            video_url = videoUrl,
            sign_id = signId,
            reference_keypoints = referenceKeypoints ?? ""
        };

        var response = await _http.PostAsJsonAsync($"{_aiBaseUrl}/analyze", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AIAnalysisResult>()
               ?? throw new Exception("Empty AI response");
    }

    public async Task<ExtractReferenceResult> ExtractReferenceKeypointsAsync(string videoUrl)
    {
        var payload = new { video_url = videoUrl };
        var response = await _http.PostAsJsonAsync($"{_aiBaseUrl}/extract-reference", payload);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExtractReferenceResult>()
               ?? throw new Exception("Empty AI response from extract-reference");
    }
}
