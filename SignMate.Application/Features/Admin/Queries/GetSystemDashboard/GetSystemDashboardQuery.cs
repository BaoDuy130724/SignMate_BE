using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Admin;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Truy vấn dashboard tổng quan toàn hệ thống — <c>GET /api/admin/dashboard</c>, chỉ SuperAdmin.
/// (CenterAdmin có dashboard riêng theo trung tâm: <c>GET /api/centers/{id}/dashboard</c>.)
/// </summary>
public record GetSystemDashboardQuery : IQuery<SystemDashboardDto>;
