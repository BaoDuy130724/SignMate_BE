using FluentValidation;

namespace SignMate.Application.Features.Center.Commands.CreateCenterUser;

/// <summary>
/// Kiểm tra hợp lệ khi tạo tài khoản trung tâm: email đúng định dạng, mật khẩu tối thiểu 6 ký tự, có họ tên.
/// </summary>
public class CreateCenterUserCommandValidator : AbstractValidator<CreateCenterUserCommand>
{
    public CreateCenterUserCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có tối thiểu 6 ký tự.");

        RuleFor(x => x.Request.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.");
    }
}
