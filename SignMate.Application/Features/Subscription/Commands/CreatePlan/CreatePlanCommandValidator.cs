using FluentValidation;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.CreatePlan;

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên gói không được để trống.")
            .MaximumLength(200);

        RuleFor(x => x.Request.PriceVnd)
            .GreaterThanOrEqualTo(0).WithMessage("Giá gói không được âm.");

        RuleFor(x => x.Request.DurationDays)
            .GreaterThan(0).WithMessage("Thời hạn gói phải lớn hơn 0 ngày.");

        RuleFor(x => x.Request.Type)
            .Must(t => Enum.TryParse<PlanType>(t, ignoreCase: true, out _))
            .WithMessage("Loại gói không hợp lệ. Các giá trị hợp lệ: Free, Basic, Pro, B2B.");
    }
}
