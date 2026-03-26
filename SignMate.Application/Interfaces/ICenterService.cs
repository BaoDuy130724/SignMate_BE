using SignMate.Application.DTOs.Center;

namespace SignMate.Application.Interfaces;

public interface ICenterService
{
    Task<List<CenterDto>> GetCentersAsync();
    Task<CenterDto> CreateCenterAsync(CenterDto center);
    Task<CenterDashboardDto> GetCenterDashboardAsync(Guid centerId);
}
