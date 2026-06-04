using FluentValidation;

namespace SignMate.Application.Features.Vocabulary.Commands.SetSignReference;

/// <summary>
/// Kiểm tra hợp lệ: phải có Id từ vựng và chuỗi keypoint mẫu không rỗng.
/// </summary>
public class SetSignReferenceCommandValidator : AbstractValidator<SetSignReferenceCommand>
{
    public SetSignReferenceCommandValidator()
    {
        RuleFor(x => x.Request.SignId).GreaterThan(0).WithMessage("SignId không hợp lệ.");
        RuleFor(x => x.Request.ReferenceKeypointData)
            .NotEmpty().WithMessage("Dữ liệu keypoint mẫu không được để trống.");
    }
}
