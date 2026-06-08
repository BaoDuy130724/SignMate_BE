using FluentValidation;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Kiểm tra hợp lệ khi cập nhật người dùng: vai trò (nếu gửi) phải nằm trong tập hợp lệ.
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Request.FullName)
            .MaximumLength(200);

        RuleFor(x => x.Request.Role)
            .Must(role => role is null || Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Vai trò không hợp lệ.");
    }
}
