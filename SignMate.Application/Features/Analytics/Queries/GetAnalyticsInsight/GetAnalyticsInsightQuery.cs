using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Features.Analytics.Queries.GetAnalyticsInsight;

/// <summary>
/// Sinh nhận định AI từ số liệu phân tích toàn cục (SuperAdmin) — <c>GET /api/analytics/insight</c>.
/// Kết quả được cache; <paramref name="ForceRefresh"/>=true để tạo lại (nút "Làm mới" trên web).
/// </summary>
public record GetAnalyticsInsightQuery(bool ForceRefresh = false) : IQuery<AdminInsightDto>;
