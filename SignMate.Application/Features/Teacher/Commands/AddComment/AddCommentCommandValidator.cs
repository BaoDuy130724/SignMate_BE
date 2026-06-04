using FluentValidation;

namespace SignMate.Application.Features.Teacher.Commands.AddComment;

/// <summary>
/// Kiểm tra hợp lệ: phải chỉ định học viên và nội dung nhận xét không rỗng.
/// </summary>
public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0).WithMessage("StudentId không hợp lệ.");
        RuleFor(x => x.Request.Content).NotEmpty().WithMessage("Nội dung nhận xét không được để trống.");
    }
}
