using FluentValidation;

namespace SignMate.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu đổi mật khẩu: phải có mật khẩu hiện tại và mật khẩu mới tối thiểu 6 ký tự.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Request.CurrentPassword)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.");

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có tối thiểu 6 ký tự.");
    }
}
