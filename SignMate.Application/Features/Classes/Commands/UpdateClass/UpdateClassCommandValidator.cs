using FluentValidation;

namespace SignMate.Application.Features.Classes.Commands.UpdateClass;

/// <summary>
/// Kiểm tra hợp lệ cho UpdateClassCommand.
/// </summary>
public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Tên lớp không được để trống.");
        RuleFor(x => x.Request.TeacherId).GreaterThan(0).WithMessage("Giáo viên chỉ định không hợp lệ.");
    }
}
