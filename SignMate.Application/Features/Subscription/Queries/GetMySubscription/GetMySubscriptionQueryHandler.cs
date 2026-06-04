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
