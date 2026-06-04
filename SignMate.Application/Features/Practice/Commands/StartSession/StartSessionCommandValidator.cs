using FluentValidation;

namespace SignMate.Application.Features.Practice.Commands.StartSession;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu bắt đầu phiên luyện tập: ký hiệu phải hợp lệ.
/// </summary>
public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.SignId)
            .GreaterThan(0).WithMessage("SignId không hợp lệ.");
    }
}
