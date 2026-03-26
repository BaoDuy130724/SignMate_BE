using SignMate.Application.DTOs.Dashboard;

namespace SignMate.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    Task<ProgressStatsDto> GetProgressStatsAsync(Guid userId);
}
