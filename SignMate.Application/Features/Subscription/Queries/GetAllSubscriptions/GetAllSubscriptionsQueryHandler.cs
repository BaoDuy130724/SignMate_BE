using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;

/// <summary>
/// Handler liệt kê tất cả lượt đăng ký gói kèm thông tin người dùng và trung tâm (nếu có),
/// phục vụ báo cáo doanh thu của quản trị viên.
/// </summary>
public class GetAllSubscriptionsQueryHandler
    : IRequestHandler<GetAllSubscriptionsQuery, List<SubscriptionListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSubscriptionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<SubscriptionListItemDto>> Handle(
        GetAllSubscriptionsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SubscriptionListItemDto
            {
                Id = s.Id,
                UserFullName = s.User.FullName,
                Email = s.User.Email,
                CenterName = s.User.Center != null ? s.User.Center.Name : null,
                PlanName = s.Plan.Name,
                PriceVnd = s.Plan.PriceVnd,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
