using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Queries.GetMySubscription;

/// <summary>
/// Handler đọc gói cước còn hiệu lực (IsActive và chưa hết hạn) của người dùng.
/// Ném 404 khi người dùng chưa có gói trả phí nào đang hoạt động (client coi như Free).
/// </summary>
public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, MySubscriptionDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMySubscriptionQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<MySubscriptionDto> Handle(GetMySubscriptionQuery query, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .AsNoTracking()
            .Include(u => u.Center)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("Người dùng không tồn tại.");

        // Nếu là Admin Trung tâm hoặc Giáo viên, kiểm tra xem Trung tâm có hoạt động không.
        // Nếu có, tự động trả về thông tin gói B2B giả lập tương ứng với trạng thái hoạt động của Trung tâm.
        if ((user.Role == UserRole.CenterAdmin || user.Role == UserRole.Teacher) && user.CenterId != null)
        {
            if (user.Center == null || !user.Center.IsActive)
            {
                throw new NotFoundException("Trung tâm đã bị ngưng hoạt động hoặc chưa được kích hoạt.");
            }

            var b2bPlan = await _unitOfWork.Repository<SubscriptionPlan>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Type == PlanType.B2B, cancellationToken);

            return new MySubscriptionDto
            {
                PlanId = b2bPlan?.Id ?? 0,
                PlanName = b2bPlan?.Name ?? "Gói Trung tâm (B2B)",
                PlanType = PlanType.B2B.ToString(),
                StartDate = user.Center.CreatedAt,
                EndDate = DateTime.UtcNow.AddYears(10), // Gói B2B của trung tâm không hết hạn qua UserSubscription lẻ
                IsActive = true
            };
        }

        return await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Where(s => s.UserId == query.UserId && s.IsActive && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .Select(s => new MySubscriptionDto
            {
                PlanId = s.PlanId,
                PlanName = s.Plan.Name,
                PlanType = s.Plan.Type.ToString(),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Người dùng chưa có gói cước nào đang hoạt động.");
    }
}
