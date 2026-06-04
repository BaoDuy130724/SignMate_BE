using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Center;

namespace SignMate.Application.Features.Center.Queries.GetCenterDashboard;

/// <summary>
/// Truy vấn số liệu giám sát của một trung tâm — <c>GET /api/centers/{id}/dashboard</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
public record GetCenterDashboardQuery(int CenterId) : IQuery<CenterDashboardDto>;
