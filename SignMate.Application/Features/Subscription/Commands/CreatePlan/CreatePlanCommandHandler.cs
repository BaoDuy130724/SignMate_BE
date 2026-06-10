using MediatR;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.CreatePlan;

/// <summary>
/// Handler cho <see cref="CreatePlanCommand"/>: tạo gói cước mới.
/// </summary>
public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, SubscriptionPlanDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<SubscriptionPlanDto> Handle(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        Enum.TryParse<PlanType>(req.Type, ignoreCase: true, out var planType);

        var plan = new SubscriptionPlan
        {
            Name = req.Name,
            PriceVnd = req.PriceVnd,
            DurationDays = req.DurationDays,
            Type = planType,
            FeaturesJson = req.FeaturesJson
        };

        await _unitOfWork.Repository<SubscriptionPlan>().AddAsync(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            PriceVnd = plan.PriceVnd,
            DurationDays = plan.DurationDays,
            Type = plan.Type.ToString(),
            FeaturesJson = plan.FeaturesJson
        };
    }
}
