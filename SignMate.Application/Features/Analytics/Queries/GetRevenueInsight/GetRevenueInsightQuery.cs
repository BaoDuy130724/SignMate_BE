using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Features.Analytics.Queries.GetRevenueInsight;

/// <summary>
/// Nhận định AI về doanh thu (SuperAdmin) — <c>GET /api/analytics/revenue-insight</c>.
/// Cache như insight phân tích; <paramref name="ForceRefresh"/>=true để tạo lại.
/// </summary>
public record GetRevenueInsightQuery(bool ForceRefresh = false) : IQuery<AdminInsightDto>;
