using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Handler tổng hợp số liệu vận hành toàn hệ thống. Mọi phép đếm/lọc được đẩy xuống DB
/// (CountAsync, Distinct) để tránh nạp bản ghi vào bộ nhớ; chỉ trả về DTO kết quả cuối.
/// </summary>
public class GetSystemDashboardQueryHandler : IRequestHandler<GetSystemDashboardQuery, SystemDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSystemDashboardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<SystemDashboardDto> Handle(GetSystemDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var totalUsers = await _unitOfWork.Repository<User>().Query()
            .CountAsync(cancellationToken);

        var activeCenters = await _unitOfWork.Repository<Domain.Entities.Center>().Query()
            .CountAsync(c => c.IsActive, cancellationToken);

        // Người dùng gói Pro: học viên có subscription đang hiệu lực và đúng loại gói.
        var premiumUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            u.UserSubscriptions != null &&
            u.UserSubscriptions.IsActive &&
            u.UserSubscriptions.EndDate >= now &&
            u.UserSubscriptions.Plan.Type == PlanType.Pro, cancellationToken);

        var basicUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            u.UserSubscriptions != null &&
            u.UserSubscriptions.IsActive &&
            u.UserSubscriptions.EndDate >= now &&
            u.UserSubscriptions.Plan.Type == PlanType.Basic, cancellationToken);

        // Người dùng miễn phí: không có gói, gói hết hạn/ngưng, hoặc gói loại Free.
        var freeUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            (u.UserSubscriptions == null ||
             !u.UserSubscriptions.IsActive ||
             u.UserSubscriptions.EndDate < now ||
             u.UserSubscriptions.Plan.Type == PlanType.Free), cancellationToken);

        var b2cUsers = basicUsers + premiumUsers;

        // Người dùng hoạt động 30 ngày qua: đếm distinct theo phiên luyện tập.
        var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= now.AddDays(-30))
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var retention = totalUsers > 0 ? (double)activeUsersLastMonth / totalUsers * 100 : 0;

        // Tỉ lệ chuyển đổi: học viên trả phí (B2C) trên tổng học viên.
        var totalStudents = await _unitOfWork.Repository<User>().Query()
            .CountAsync(u => u.Role == UserRole.Student, cancellationToken);
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
