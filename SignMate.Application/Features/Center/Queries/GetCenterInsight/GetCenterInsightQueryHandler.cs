using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Features.Analytics.Common;
using SignMate.Application.Features.Center.Queries.GetCenterDashboard;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Center.Queries.GetCenterInsight;

/// <summary>
/// Dựng nhận định AI cho một trung tâm: tái dùng <see cref="GetCenterDashboardQuery"/> (đã chặn IDOR —
/// CenterAdmin chỉ đọc được trung tâm mình) rồi đưa số liệu cho Gemini diễn giải. Cache 3 giờ theo centerId.
/// </summary>
public class GetCenterInsightQueryHandler : IRequestHandler<GetCenterInsightQuery, AdminInsightDto>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(3);

    private readonly IMediator _mediator;
    private readonly IGeminiService _gemini;
    private readonly IMemoryCache _cache;

    public GetCenterInsightQueryHandler(IMediator mediator, IGeminiService gemini, IMemoryCache cache)
    {
        _mediator = mediator;
        _gemini = gemini;
        _cache = cache;
    }

    public async Task<AdminInsightDto> Handle(GetCenterInsightQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"center-insight:{request.CenterId}";
        if (!request.ForceRefresh && _cache.TryGetValue(cacheKey, out AdminInsightDto? cached) && cached != null)
            return cached;

        // Reuse dashboard query → chính nó kiểm tra quyền (CenterAdmin chỉ được center mình).
        var data = await _mediator.Send(new GetCenterDashboardQuery(request.CenterId), cancellationToken);
        var prompt = BuildPrompt(data);
        var insight = await AdminInsightBuilder.BuildAsync(_gemini, prompt);

        if (insight.AiAvailable)
            _cache.Set(cacheKey, insight, CacheTtl);

        return insight;
    }

    private static string BuildPrompt(CenterDashboardDto d)
    {
        var ci = CultureInfo.InvariantCulture;
        var fillRate = d.MaxSeats > 0 ? Math.Round((double)d.TotalStudents / d.MaxSeats * 100, 1) : 0;
        var activeRate = d.TotalStudents > 0 ? Math.Round((double)d.ActiveLearners / d.TotalStudents * 100, 1) : 0;

        var sb = new StringBuilder();
        sb.AppendLine("Bạn là cố vấn vận hành cho một TRUNG TÂM dạy ngôn ngữ ký hiệu (đối tác B2B của nền tảng SignMate).");
        sb.AppendLine("Dưới đây là số liệu THẬT của riêng trung tâm này. Hãy đưa ra nhận định cho quản trị viên trung tâm (CenterAdmin).");
        sb.AppendLine("Chỉ nói về trung tâm này, không so sánh với hệ thống chung.");
        sb.AppendLine();
        sb.AppendLine($"- Trung tâm: {d.CenterName}");
        sb.AppendLine($"- Sĩ số: {d.TotalStudents}/{d.MaxSeats} chỗ (lấp đầy {fillRate.ToString(ci)}%)");
        sb.AppendLine($"- Học viên có luyện tập: {d.ActiveLearners} ({activeRate.ToString(ci)}% sĩ số)");
        sb.AppendLine($"- Độ chính xác luyện tập trung bình: {d.AverageAccuracy.ToString(ci)}%");
        sb.AppendLine($"- Tổng thời lượng luyện tập (phiên đã kết thúc): {d.TotalPracticeMinutes} phút");
        sb.AppendLine($"- Học viên mới trong tháng này: {d.NewStudentsThisMonth}");
        sb.AppendLine();
        sb.AppendLine(AdminInsightBuilder.JsonInstruction);
        return sb.ToString();
    }
}
