using FluentValidation;

namespace SignMate.Application.Features.Subscription.Commands.UpdatePlan;

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID gói không hợp lệ.");

        When(x => x.Request.Name is not null, () =>
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Tên gói không được để trống.")
                .MaximumLength(200));

        When(x => x.Request.PriceVnd.HasValue, () =>
            RuleFor(x => x.Request.PriceVnd!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Giá gói không được âm."));

        When(x => x.Request.DurationDays.HasValue, () =>
            RuleFor(x => x.Request.DurationDays!.Value)
                .GreaterThan(0).WithMessage("Thời hạn gói phải lớn hơn 0 ngày."));
    }
}
