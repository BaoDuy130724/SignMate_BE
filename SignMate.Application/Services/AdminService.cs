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
        
        // Correct query based on validity dates and Enum PlanType
        var premiumUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u => 
            u.Role == UserRole.Student && 
            u.UserSubscriptions != null && 
            u.UserSubscriptions.IsActive && 
            u.UserSubscriptions.EndDate >= DateTime.UtcNow &&
            u.UserSubscriptions.Plan.Type == PlanType.Pro); 
            
        var basicUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u => 
            u.Role == UserRole.Student && 
            u.UserSubscriptions != null && 
            u.UserSubscriptions.IsActive && 
            u.UserSubscriptions.EndDate >= DateTime.UtcNow &&
            u.UserSubscriptions.Plan.Type == PlanType.Basic);

        var freeUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u => 
            u.Role == UserRole.Student && 
            (u.UserSubscriptions == null || 
             !u.UserSubscriptions.IsActive || 
             u.UserSubscriptions.EndDate < DateTime.UtcNow ||
             u.UserSubscriptions.Plan.Type == PlanType.Free));

        var b2cUsers = basicUsers + premiumUsers;

        var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= DateTime.UtcNow.AddDays(-30))
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync();
            
        var retention = totalUsers > 0 ? (double)activeUsersLastMonth / totalUsers * 100 : 0;
        
        // Conversion rate is calculated based on total B2C paid users over total student users
        var totalStudents = await _unitOfWork.Repository<User>().Query().CountAsync(u => u.Role == UserRole.Student);
        var conversion = totalStudents > 0 ? (double)b2cUsers / totalStudents * 100 : 0;

        return new SystemDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveCenters = activeCenters,
            TotalRevenue = premiumUsers * 99000m + basicUsers * 49000m,
            ConversionRate = Math.Round(conversion, 1),
            PremiumUsers = premiumUsers,
            BasicUsers = basicUsers,
            FreeUsers = freeUsers,
            B2CUsers = b2cUsers,
            RetentionRate = Math.Round(retention, 1)
        };
    }
}
