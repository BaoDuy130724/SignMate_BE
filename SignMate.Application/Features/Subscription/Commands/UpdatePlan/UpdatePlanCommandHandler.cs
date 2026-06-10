using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.UpdatePlan;

/// <summary>
/// Handler cho <see cref="UpdatePlanCommand"/>: cập nhật partial các trường của gói cước.
/// </summary>
public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, SubscriptionPlanDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<SubscriptionPlanDto> Handle(UpdatePlanCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<SubscriptionPlan>();
        var plan = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), command.Id);

        var req = command.Request;
        if (req.Name is not null) plan.Name = req.Name;
        if (req.PriceVnd.HasValue) plan.PriceVnd = req.PriceVnd.Value;
        if (req.DurationDays.HasValue) plan.DurationDays = req.DurationDays.Value;
        if (req.FeaturesJson is not null) plan.FeaturesJson = req.FeaturesJson;

        repo.Update(plan);
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
