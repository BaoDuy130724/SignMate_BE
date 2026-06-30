using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Features.Analytics.Common;
using SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Analytics.Queries.GetRevenueInsight;

/// <summary>
/// Dựng nhận định AI về doanh thu: tính các chỉ số thật từ danh sách subscription (cùng công thức
/// mà trang Revenue web đang dùng — B2B = có CenterName), rồi đưa cho Gemini diễn giải. Cache 3 giờ.
/// </summary>
public class GetRevenueInsightQueryHandler : IRequestHandler<GetRevenueInsightQuery, AdminInsightDto>
{
    private const string CacheKey = "admin:revenue-insight";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(3);

    private readonly IMediator _mediator;
    private readonly IGeminiService _gemini;
    private readonly IMemoryCache _cache;

    public GetRevenueInsightQueryHandler(IMediator mediator, IGeminiService gemini, IMemoryCache cache)
    {
        _mediator = mediator;
        _gemini = gemini;
        _cache = cache;
    }

    public async Task<AdminInsightDto> Handle(GetRevenueInsightQuery request, CancellationToken cancellationToken)
    {
        if (!request.ForceRefresh && _cache.TryGetValue(CacheKey, out AdminInsightDto? cached) && cached != null)
            return cached;

        var subs = await _mediator.Send(new GetAllSubscriptionsQuery(), cancellationToken);
        var prompt = BuildPrompt(subs);
        var insight = await AdminInsightBuilder.BuildAsync(_gemini, prompt);

        if (insight.AiAvailable)
            _cache.Set(CacheKey, insight, CacheTtl);

        return insight;
    }

    private static string BuildPrompt(List<SubscriptionListItemDto> subs)
    {
        var ci = CultureInfo.InvariantCulture;

        static bool IsB2B(SubscriptionListItemDto s) => !string.IsNullOrEmpty(s.CenterName);

        var total = subs.Sum(s => (long)s.PriceVnd);
        var b2b = subs.Where(IsB2B).Sum(s => (long)s.PriceVnd);
        var b2c = total - b2b;
        var txCount = subs.Count;
        var arpu = txCount > 0 ? total / txCount : 0;

        // MoM: tháng này so với tháng trước (gom theo StartDate, giờ UTC — đủ cho nhận định xu hướng).
        var now = DateTime.UtcNow;
        var curMonth = (now.Year, now.Month);
        var prevDate = now.AddMonths(-1);
        var prevMonth = (prevDate.Year, prevDate.Month);
        long Rev((int Year, int Month) m) =>
            subs.Where(s => s.StartDate.Year == m.Year && s.StartDate.Month == m.Month).Sum(s => (long)s.PriceVnd);
        var curRev = Rev(curMonth);
        var prevRev = Rev(prevMonth);
        var momPercent = prevRev > 0 ? Math.Round((double)(curRev - prevRev) / prevRev * 100, 1) : (curRev > 0 ? 100.0 : 0.0);

        var byPlan = subs.GroupBy(s => s.PlanName)
            .Select(g => $"{g.Key}: {g.Count()} lượt, {g.Sum(s => (long)s.PriceVnd).ToString("N0", ci)}đ")
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Bạn là chuyên gia phân tích tài chính/doanh thu cho sản phẩm SaaS giáo dục (SignMate).");
        sb.AppendLine("Dưới đây là số liệu doanh thu THẬT (tổng hợp từ các lượt đăng ký gói). Đơn vị: VNĐ.");
        sb.AppendLine("Lưu ý: đây là doanh thu ghi nhận theo subscription, không phải dòng tiền thực thu từng tháng.");
        sb.AppendLine();
        sb.AppendLine($"- Tổng doanh thu ghi nhận: {total.ToString("N0", ci)}đ từ {txCount} lượt đăng ký");
        sb.AppendLine($"- B2B (qua trung tâm): {b2b.ToString("N0", ci)}đ; B2C (cá nhân): {b2c.ToString("N0", ci)}đ");
        sb.AppendLine($"- Doanh thu trung bình/lượt (ARPU): {arpu.ToString("N0", ci)}đ");
        sb.AppendLine($"- Tháng này: {curRev.ToString("N0", ci)}đ; tháng trước: {prevRev.ToString("N0", ci)}đ; thay đổi: {momPercent.ToString(ci)}%");
        sb.AppendLine("- Theo gói:");
        foreach (var p in byPlan) sb.AppendLine($"  + {p}");
        sb.AppendLine();
        sb.AppendLine(AdminInsightBuilder.JsonInstruction);
        return sb.ToString();
    }
}
