using FluentValidation;

namespace SignMate.Application.Features.Onboarding.Commands.SubmitOnboarding;

/// <summary>
/// Kiểm tra hợp lệ cho <see cref="SubmitOnboardingCommand"/>: bắt buộc có mục tiêu và trình độ.
/// </summary>
public class SubmitOnboardingCommandValidator : AbstractValidator<SubmitOnboardingCommand>
{
    public SubmitOnboardingCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Không xác định được người dùng.");
        RuleFor(x => x.Request.Goal).NotEmpty().WithMessage("Vui lòng chọn mục tiêu học tập.");
        RuleFor(x => x.Request.Level).NotEmpty().WithMessage("Vui lòng chọn trình độ.");
    }
}
