using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.DeletePlan;

/// <summary>
/// Handler cho <see cref="DeletePlanCommand"/>: xóa gói cước nếu tồn tại.
/// Không cho xóa nếu gói đang có subscription active (tránh mất dữ liệu người dùng).
/// </summary>
public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePlanCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Unit> Handle(DeletePlanCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<SubscriptionPlan>();
        var plan = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), command.Id);

        var hasActive = _unitOfWork.Repository<UserSubscription>().Query()
            .Any(s => s.PlanId == command.Id && s.IsActive);

        if (hasActive)
            throw new InvalidOperationException("Không thể xóa gói đang có người dùng đăng ký.");

        repo.Delete(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
