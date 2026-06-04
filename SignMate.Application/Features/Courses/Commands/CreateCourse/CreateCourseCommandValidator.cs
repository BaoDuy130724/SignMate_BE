using FluentValidation;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Commands.CreateCourse;

/// <summary>
/// Kiểm tra hợp lệ khi tạo khóa học: tiêu đề bắt buộc và cấp độ phải là giá trị hợp lệ.
/// </summary>
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Tiêu đề khóa học không được để trống.")
            .MaximumLength(300);

        RuleFor(x => x.Request.Level)
            .Must(level => Enum.TryParse<CourseLevel>(level, ignoreCase: true, out _))
            .WithMessage("Cấp độ khóa học không hợp lệ.");
    }
}
