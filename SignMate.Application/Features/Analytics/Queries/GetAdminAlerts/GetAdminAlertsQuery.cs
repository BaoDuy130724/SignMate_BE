using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Features.Analytics.Queries.GetAdminAlerts;

/// <summary>
/// Phát hiện cảnh báo bất thường cho SuperAdmin bằng LUẬT trên số liệu thật — <c>GET /api/analytics/alerts</c>.
/// Deterministic (không gọi AI) nên số liệu cảnh báo luôn đáng tin.
/// </summary>
public record GetAdminAlertsQuery : IQuery<List<AdminAlertDto>>;
