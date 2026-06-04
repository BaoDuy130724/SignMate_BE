using FluentValidation;

namespace SignMate.Application.Features.Progress.Commands.UpdateLessonProgress;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu cập nhật tiến độ bài học: bài học hợp lệ, trạng thái bắt buộc,
/// thời lượng xem không âm. Việc ánh xạ trạng thái sang enum nghiệp vụ do handler đảm nhiệm.
/// </summary>
public class UpdateLessonProgressCommandValidator : AbstractValidator<UpdateLessonProgressCommand>
{
    public UpdateLessonProgressCommandValidator()
    {
        RuleFor(x => x.Request.LessonId)
            .GreaterThan(0).WithMessage("LessonId không hợp lệ.");

        RuleFor(x => x.Request.Status)
            .NotEmpty().WithMessage("Trạng thái không được để trống.");

        RuleFor(x => x.Request.WatchDurationSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Thời lượng xem không được âm.");
    }
}
