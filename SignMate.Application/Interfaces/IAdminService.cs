using SignMate.Application.DTOs.Admin;

namespace SignMate.Application.Interfaces;

public interface IAdminService
{
    Task<SystemDashboardDto> GetSystemDashboardAsync();
}
