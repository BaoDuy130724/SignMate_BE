using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Dashboard;

namespace SignMate.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Truy vấn dữ liệu tổng quan cho trang chủ học viên: streak, độ chính xác trung bình,
/// bài học gợi ý và danh sách deadline được giao — <c>GET /api/dashboard</c>.
/// </summary>
/// <param name="UserId">Id học viên lấy từ JWT.</param>
public record GetDashboardSummaryQuery(int UserId) : IQuery<DashboardSummaryDto>;
