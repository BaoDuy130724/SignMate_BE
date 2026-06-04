using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Dashboard;

namespace SignMate.Application.Features.Dashboard.Queries.GetProgressStats;

/// <summary>
/// Truy vấn thống kê tiến độ học tập của học viên: độ chính xác tổng thể, số bài hoàn thành,
/// số từ vựng đã thành thạo, độ chính xác theo chủ đề và các chủ đề còn yếu —
/// <c>GET /api/dashboard/progress</c>.
/// </summary>
/// <param name="UserId">Id học viên lấy từ JWT.</param>
public record GetProgressStatsQuery(int UserId) : IQuery<ProgressStatsDto>;
