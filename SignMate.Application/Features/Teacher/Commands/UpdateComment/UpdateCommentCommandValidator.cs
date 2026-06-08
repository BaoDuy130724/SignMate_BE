using FluentValidation;

namespace SignMate.Application.Features.Teacher.Commands.UpdateComment;

/// <summary>
/// Kiểm tra hợp lệ: nội dung nhận xét mới không được để trống.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Request.Content).NotEmpty().WithMessage("Nội dung nhận xét không được để trống.");
    }
}
