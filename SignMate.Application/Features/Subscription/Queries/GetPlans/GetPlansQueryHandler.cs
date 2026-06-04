using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Queries.GetPlans;

/// <summary>
/// Handler liệt kê toàn bộ gói cước. Trả nguyên <c>FeaturesJson</c> để client tự giải mã danh sách
/// tính năng hiển thị dạng gạch đầu dòng.
/// </summary>
public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPlansQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<SubscriptionPlanDto>> Handle(GetPlansQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<SubscriptionPlan>().Query()
            .AsNoTracking()
            .OrderBy(p => p.PriceVnd)
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id,
                Name = p.Name,
                PriceVnd = p.PriceVnd,
                DurationDays = p.DurationDays,
                Type = p.Type.ToString(),
                FeaturesJson = p.FeaturesJson
            })
            .ToListAsync(cancellationToken);
    }
}
