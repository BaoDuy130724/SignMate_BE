using FluentValidation;

namespace SignMate.Application.Features.Center.Commands.UpdateCenterMember;

/// <summary>
/// Kiểm tra hợp lệ cho UpdateCenterMemberCommand.
/// </summary>
public class UpdateCenterMemberCommandValidator : AbstractValidator<UpdateCenterMemberCommand>
{
    public UpdateCenterMemberCommandValidator()
    {
        RuleFor(x => x.Request.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(200).WithMessage("Họ tên không được vượt quá 200 ký tự.");
    }
}
