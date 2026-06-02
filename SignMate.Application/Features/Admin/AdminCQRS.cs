using MediatR;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Admin;

public record GetSystemDashboardQuery : IRequest<SystemDashboardDto>;

public class AdminCQRSHandlers : IRequestHandler<GetSystemDashboardQuery, SystemDashboardDto>
{
    private readonly IAdminService _adminService;

    public AdminCQRSHandlers(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<SystemDashboardDto> Handle(GetSystemDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetSystemDashboardAsync();
    }
}
