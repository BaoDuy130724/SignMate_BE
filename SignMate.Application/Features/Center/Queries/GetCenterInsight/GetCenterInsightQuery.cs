using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Features.Center.Queries.GetCenterInsight;

/// <summary>
/// Nhận định AI cho MỘT trung tâm (CenterAdmin xem trung tâm mình; SuperAdmin xem bất kỳ) —
/// <c>GET /api/centers/{id}/insight</c>. Scope multi-tenant được thực thi lại ở
/// <see cref="SignMate.Application.Features.Center.Queries.GetCenterDashboard.GetCenterDashboardQuery"/>
/// (reuse), nên CenterAdmin không thể đọc trung tâm khác. Cache theo từng center.
/// </summary>
public record GetCenterInsightQuery(int CenterId, bool ForceRefresh = false) : IQuery<AdminInsightDto>;
