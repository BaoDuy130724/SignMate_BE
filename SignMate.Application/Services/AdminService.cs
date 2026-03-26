using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Services;

public class AdminService : IAdminService
{
    private readonly ISignMateDbContext _db;

    public AdminService(ISignMateDbContext db) => _db = db;

    public async Task<SystemDashboardDto> GetSystemDashboardAsync()
    {
        var totalUsers = await _db.Users.CountAsync();
        var activeCenters = await _db.Centers.CountAsync(c => c.IsActive);

        return new SystemDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveCenters = activeCenters,
            TotalRevenue = 15000000m, // Mock revenue sum
            ConversionRate = 5.2 // Mock 5.2%
        };
    }
}
