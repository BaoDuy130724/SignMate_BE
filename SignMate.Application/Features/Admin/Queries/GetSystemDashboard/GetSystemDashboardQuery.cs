using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Admin;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Truy vấn dashboard tổng quan — <c>GET /api/admin/dashboard</c>.
/// SuperAdmin: toàn hệ thống. CenterAdmin: truyền CallerCenterId để lọc theo trung tâm.
/// </summary>
/// <param name="CallerCenterId">Id trung tâm của caller; null = SuperAdmin (toàn hệ thống).</param>
public record GetSystemDashboardQuery(int? CallerCenterId = null) : IQuery<SystemDashboardDto>;
