using SignMate.Application.DTOs.Analytics;

namespace SignMate.Application.Interfaces;

public interface IAnalyticsService
{
    Task<GlobalAnalyticsDto> GetGlobalAnalyticsAsync();
}
