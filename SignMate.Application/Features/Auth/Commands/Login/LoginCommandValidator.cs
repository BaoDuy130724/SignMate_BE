using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Kiểm tra hợp lệ biểu mẫu đăng nhập, phản chiếu ràng buộc phía client:
/// email bắt buộc &amp; đúng định dạng, mật khẩu tối thiểu 6 ký tự.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có tối thiểu 6 ký tự.");
    }
}
