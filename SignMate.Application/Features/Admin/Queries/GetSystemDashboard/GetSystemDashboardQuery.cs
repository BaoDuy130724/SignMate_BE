using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Admin;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Truy vấn dashboard tổng quan toàn hệ thống cho SuperAdmin: tổng người dùng, doanh thu,
/// phân bổ gói (Free/Basic/Pro), tỉ lệ chuyển đổi và giữ chân — <c>GET /api/admin/dashboard</c>.
/// </summary>
public record GetSystemDashboardQuery : IQuery<SystemDashboardDto>;
