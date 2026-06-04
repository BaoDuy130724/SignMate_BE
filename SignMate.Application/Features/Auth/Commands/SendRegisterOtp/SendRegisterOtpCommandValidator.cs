using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.SendRegisterOtp;

/// <summary>
/// Kiểm tra email hợp lệ trước khi phát hành OTP đăng ký.
/// </summary>
public class SendRegisterOtpCommandValidator : AbstractValidator<SendRegisterOtpCommand>
{
    public SendRegisterOtpCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");
    }
}
