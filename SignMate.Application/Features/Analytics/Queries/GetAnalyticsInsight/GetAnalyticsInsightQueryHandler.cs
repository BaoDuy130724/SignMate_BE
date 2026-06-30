using System.Globalization;
using System.Text;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Features.Analytics.Common;
using SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Analytics.Queries.GetAnalyticsInsight;

/// <summary>
/// Dựng nhận định AI cho trang Analytics: tái dùng số liệu thật của <see cref="GetGlobalAnalyticsQuery"/>,
/// đưa cho Gemini diễn giải. Kết quả cache 3 giờ (IMemoryCache) để không gọi AI mỗi lần load trang.
/// </summary>
public class GetAnalyticsInsightQueryHandler : IRequestHandler<GetAnalyticsInsightQuery, AdminInsightDto>
{
    private const string CacheKey = "admin:analytics-insight";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(3);

    private readonly IMediator _mediator;
    private readonly IGeminiService _gemini;
    private readonly IMemoryCache _cache;

    public GetAnalyticsInsightQueryHandler(IMediator mediator, IGeminiService gemini, IMemoryCache cache)
    {
        _mediator = mediator;
        _gemini = gemini;
        _cache = cache;
    }

    public async Task<AdminInsightDto> Handle(GetAnalyticsInsightQuery request, CancellationToken cancellationToken)
    {
        if (!request.ForceRefresh && _cache.TryGetValue(CacheKey, out AdminInsightDto? cached) && cached != null)
            return cached;

        var data = await _mediator.Send(new GetGlobalAnalyticsQuery(), cancellationToken);
        var prompt = BuildPrompt(data);
        var insight = await AdminInsightBuilder.BuildAsync(_gemini, prompt);

        // Chỉ cache khi AI thật sự trả nội dung (tránh "khóa" trạng thái lỗi 3 giờ).
        if (insight.AiAvailable)
            _cache.Set(CacheKey, insight, CacheTtl);

        return insight;
    }

    private static string BuildPrompt(GlobalAnalyticsDto d)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là chuyên gia phân tích dữ liệu sản phẩm EdTech (ứng dụng học ngôn ngữ ký hiệu SignMate).");
        sb.AppendLine("Dưới đây là số liệu vận hành THẬT của hệ thống. Hãy đưa ra nhận định cho quản trị viên (SuperAdmin).");
        sb.AppendLine();
        sb.AppendLine("== SỐ LIỆU TỔNG QUAN ==");
        sb.AppendLine($"- Tổng người dùng: {d.TotalUsers}");
        sb.AppendLine($"- Trong đó B2B (qua trung tâm): {d.B2BUsers}; B2C (cá nhân): {d.TotalUsers - d.B2BUsers}");
        sb.AppendLine($"- Tổng trung tâm đối tác: {d.TotalCenters}");
        sb.AppendLine($"- Người dùng hoạt động 30 ngày: {d.ActiveUsersLast30Days}");
        sb.AppendLine($"- Độ chính xác luyện tập trung bình: {d.AverageAccuracy.ToString(ci)}%");
        sb.AppendLine($"- Tổng phiên luyện tập: {d.TotalPracticeSessions}; lượt đạt (>=80%): {d.TotalSuccessfulAttempts}");
        sb.AppendLine($"- Tăng trưởng phiên 30 ngày gần nhất so với 30 ngày trước: {d.SessionGrowthPercent.ToString(ci)}%");
        sb.AppendLine($"- Hôm nay: {d.SessionsToday} phiên, {d.AttemptsToday} lượt AI chấm, {d.ActiveUsersToday} người hoạt động");
        sb.AppendLine();
        sb.AppendLine("== TOP KHÓA HỌC (theo lượt ghi danh) ==");
        if (d.TopCourses.Count == 0)
        {
            sb.AppendLine("(chưa có khóa học nào có người ghi danh)");
        }
        else
        {
            foreach (var c in d.TopCourses)
                sb.AppendLine($"- {c.Name}: {c.Enrollments} ghi danh, hoàn thành {c.CompletionRate.ToString(ci)}%, mới 30 ngày: {c.NewEnrollmentsLast30Days}");
        }
        sb.AppendLine();
        sb.AppendLine(AdminInsightBuilder.JsonInstruction);
        return sb.ToString();
    }
}
