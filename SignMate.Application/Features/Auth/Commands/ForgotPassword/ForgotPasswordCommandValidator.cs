using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Kiểm tra email hợp lệ trước khi phát hành OTP khôi phục mật khẩu.
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");
    }
}
