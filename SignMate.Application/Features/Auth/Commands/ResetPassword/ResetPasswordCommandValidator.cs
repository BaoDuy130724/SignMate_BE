using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu đặt lại mật khẩu: email, mã OTP 6 ký tự, mật khẩu mới tối thiểu 6 ký tự.
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Request.Token)
            .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
            .Length(6).WithMessage("Mã OTP phải gồm 6 ký tự.");

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có tối thiểu 6 ký tự.");
    }
}
