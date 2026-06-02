using SignMate.Application.DTOs.Dashboard;

namespace SignMate.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(int userId);
    Task<ProgressStatsDto> GetProgressStatsAsync(int userId);
}
