using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.Register;

/// <summary>
/// Kiểm tra hợp lệ dữ liệu đăng ký: email, mật khẩu tối thiểu 6 ký tự, họ tên, và OTP đúng 6 số.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có tối thiểu 6 ký tự.");

        RuleFor(x => x.Request.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.");

        RuleFor(x => x.Request.OtpCode)
            .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
            .Length(6).WithMessage("Mã OTP phải gồm 6 ký tự.");
    }
}
