using FluentValidation;

namespace SignMate.Application.Features.Progress.Commands.UpdateSignProgress;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu cập nhật tiến độ ký hiệu: ký hiệu phải hợp lệ.
/// </summary>
public class UpdateSignProgressCommandValidator : AbstractValidator<UpdateSignProgressCommand>
{
    public UpdateSignProgressCommandValidator()
    {
        RuleFor(x => x.Request.SignId)
            .GreaterThan(0).WithMessage("SignId không hợp lệ.");
    }
}
