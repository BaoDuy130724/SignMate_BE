using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;

/// <summary>
/// Truy vấn số liệu phân tích toàn cục cho SuperAdmin: tổng quan người dùng/trung tâm,
/// tăng trưởng 30 ngày, phân bổ B2B/B2C và top khóa học — <c>GET /api/analytics</c>.
/// </summary>
public record GetGlobalAnalyticsQuery : IQuery<GlobalAnalyticsDto>;
