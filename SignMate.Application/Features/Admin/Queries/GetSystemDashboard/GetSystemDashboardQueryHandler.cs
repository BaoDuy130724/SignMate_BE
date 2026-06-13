using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Admin;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Admin.Queries.GetSystemDashboard;

/// <summary>
/// Handler tổng hợp số liệu vận hành toàn hệ thống (chỉ SuperAdmin).
/// - B2B: student CÓ centerId (không phụ thuộc UserSubscription).
/// - B2C: student KHÔNG có centerId (Free/Basic/Pro theo UserSubscription).
/// </summary>
public class GetSystemDashboardQueryHandler : IRequestHandler<GetSystemDashboardQuery, SystemDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSystemDashboardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<SystemDashboardDto> Handle(GetSystemDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // ── B2B: student có centerId ─────────────────────────────────────────
        var b2bUsers = await _unitOfWork.Repository<User>().Query()
            .CountAsync(u => u.Role == UserRole.Student && u.CenterId != null, cancellationToken);

        var totalUsers = await _unitOfWork.Repository<User>().Query()
            .CountAsync(cancellationToken);

        var activeCenters = await _unitOfWork.Repository<SignMate.Domain.Entities.Center>().Query()
            .CountAsync(c => c.IsActive, cancellationToken);

        // ── B2C: student không có centerId ───────────────────────────────────
        var premiumUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            u.CenterId == null &&
            u.UserSubscriptions.Any(s =>
                s.IsActive &&
                s.EndDate >= now &&
                s.Plan.Type == PlanType.Pro), cancellationToken);

        var basicUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            u.CenterId == null &&
            u.UserSubscriptions.Any(s =>
                s.IsActive &&
                s.EndDate >= now &&
                s.Plan.Type == PlanType.Basic), cancellationToken);

        var freeUsers = await _unitOfWork.Repository<User>().Query().CountAsync(u =>
            u.Role == UserRole.Student &&
            u.CenterId == null &&
            !u.UserSubscriptions.Any(s =>
                s.IsActive &&
                s.EndDate >= now &&
                (s.Plan.Type == PlanType.Pro || s.Plan.Type == PlanType.Basic)), cancellationToken);

        var activeUsersLastMonth = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= now.AddDays(-30))
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Retention: tử số chỉ gồm người có luyện tập (student), nên mẫu số
        // phải là tổng student — chia cho totalUsers (gồm cả admin/teacher)
        // sẽ pha loãng, cho retention thấp giả.
        var totalStudents = await _unitOfWork.Repository<User>().Query()
            .CountAsync(u => u.Role == UserRole.Student, cancellationToken);
        var retention = totalStudents > 0 ? (double)activeUsersLastMonth / totalStudents * 100 : 0;

        // Conversion B2C: paid B2C trên tổng student B2C (free + basic + pro).
        // Student B2B (có centerId) không thể "convert" nên không thuộc mẫu số.
        var b2cStudents = premiumUsers + basicUsers + freeUsers;
        var paidB2C = premiumUsers + basicUsers;
        var conversion = b2cStudents > 0 ? (double)paidB2C / b2cStudents * 100 : 0;

        // Doanh thu (MRR ước tính): giá đọc từ bảng SubscriptionPlan để luôn khớp giá đang bán,
        // không hardcode — đổi giá trong DB là dashboard tự đúng.
        var prices = await _unitOfWork.Repository<SubscriptionPlan>().Query()
            .ToDictionaryAsync(p => p.Type, p => p.PriceVnd, cancellationToken);
        var revenue = premiumUsers * prices.GetValueOrDefault(PlanType.Pro)
                    + basicUsers * prices.GetValueOrDefault(PlanType.Basic)
                    + b2bUsers * prices.GetValueOrDefault(PlanType.B2B);

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
