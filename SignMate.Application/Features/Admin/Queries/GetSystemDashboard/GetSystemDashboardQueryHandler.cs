using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Handler tổng hợp số liệu vận hành.
/// - B2B: student CÓ centerId (không phụ thuộc UserSubscription).
/// - B2C: student KHÔNG có centerId (Free/Basic/Pro theo UserSubscription).
/// Khi CallerCenterId được truyền (CenterAdmin): chỉ tính student trong center đó — tất cả đều là B2B.
/// </summary>
public class GetSystemDashboardQueryHandler : IRequestHandler<GetSystemDashboardQuery, SystemDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSystemDashboardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<SystemDashboardDto> Handle(GetSystemDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var centerId = request.CallerCenterId;

        // ── B2B: student có centerId ─────────────────────────────────────────
        var b2bQuery = _unitOfWork.Repository<User>().Query()
            .Where(u => u.Role == UserRole.Student && u.CenterId != null);
        if (centerId.HasValue)
            b2bQuery = b2bQuery.Where(u => u.CenterId == centerId.Value);
        var b2bUsers = await b2bQuery.CountAsync(cancellationToken);

        // ── B2C: student không có centerId (chỉ SuperAdmin thấy) ─────────────
        int premiumUsers = 0, basicUsers = 0, freeUsers = 0, totalUsers = 0, activeCenters = 0;
        double retention = 0, conversion = 0;

        if (!centerId.HasValue)
        {
            // Toàn hệ thống (SuperAdmin)
            totalUsers = await _unitOfWork.Repository<User>().Query()
                .CountAsync(cancellationToken);

            activeCenters = await _unitOfWork.Repository<SignMate.Domain.Entities.Center>().Query()
                .CountAsync(c => c.IsActive, cancellationToken);

            premiumUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
                u.Role == UserRole.Student &&
                u.CenterId == null &&
                u.UserSubscriptions != null &&
                u.UserSubscriptions.IsActive &&
                u.UserSubscriptions.EndDate >= now &&
                u.UserSubscriptions.Plan.Type == PlanType.Pro, cancellationToken);

            basicUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
                u.Role == UserRole.Student &&
                u.CenterId == null &&
                u.UserSubscriptions != null &&
                u.UserSubscriptions.IsActive &&
                u.UserSubscriptions.EndDate >= now &&
                u.UserSubscriptions.Plan.Type == PlanType.Basic, cancellationToken);

            freeUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
                u.Role == UserRole.Student &&
                u.CenterId == null &&
                (u.UserSubscriptions == null ||
                 !u.UserSubscriptions.IsActive ||
                 u.UserSubscriptions.EndDate < now ||
                 u.UserSubscriptions.Plan.Type == PlanType.Free), cancellationToken);

            var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
                .Where(ps => ps.StartedAt >= now.AddDays(-30))
                .Select(ps => ps.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            retention = totalUsers > 0 ? (double)activeUsersLastMonth / totalUsers * 100 : 0;

            var totalStudents = await _unitOfWork.Repository<User>().Query()
                .CountAsync(u => u.Role == UserRole.Student, cancellationToken);
            var paidB2C = premiumUsers + basicUsers;
            conversion = totalStudents > 0 ? (double)paidB2C / totalStudents * 100 : 0;
        }
        else
        {
            // CenterAdmin: chỉ xem center của mình
            totalUsers = b2bUsers;
            activeCenters = 1;

            var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
                .Where(ps => ps.StartedAt >= now.AddDays(-30) && ps.User.CenterId == centerId.Value)
                .Select(ps => ps.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            retention = b2bUsers > 0 ? (double)activeUsersLastMonth / b2bUsers * 100 : 0;
            conversion = 100; // tất cả học viên trung tâm đều trả phí (B2B)
        }

        var revenue = premiumUsers * 99_000m + basicUsers * 49_000m + b2bUsers * 79_000m;

        return new SystemDashboardDto
        {
            TotalUsers = totalUsers,
            ActiveCenters = activeCenters,
            TotalRevenue = revenue,
            ConversionRate = Math.Round(conversion, 1),
            PremiumUsers = premiumUsers,
            BasicUsers = basicUsers,
            FreeUsers = freeUsers,
            B2CUsers = premiumUsers + basicUsers,
            B2BUsers = b2bUsers,
            RetentionRate = Math.Round(retention, 1)
        };
    }
}
