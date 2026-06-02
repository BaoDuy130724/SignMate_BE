using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<SystemDashboardDto> GetSystemDashboardAsync()
    {
        var totalUsers = await _unitOfWork.Repository<User>().Query().CountAsync();
        var activeCenters = await _unitOfWork.Repository<Center>().Query().CountAsync(c => c.IsActive);
        
        // Simulating retention and premium users based on active practice sessions
        var premiumUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u => u.Role == UserRole.Student && u.XpPoints > 500); 
        var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= DateTime.UtcNow.AddDays(-30))
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync();
            
        var retention = totalUsers > 0 ? (double)activeUsersLastMonth / totalUsers * 100 : 0;
        var conversion = totalUsers > 0 ? (double)premiumUsers / totalUsers * 100 : 0;

        return new SystemDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveCenters = activeCenters,
            TotalRevenue = premiumUsers * 120000m, // Derived: 120,000 VND per premium user
            ConversionRate = Math.Round(conversion, 1),
            PremiumUsers = premiumUsers,
            RetentionRate = Math.Round(retention, 1)
        };
    }
}
