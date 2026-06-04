using FluentValidation;

namespace SignMate.Application.Features.B2BContact.Commands.SubmitContact;

/// <summary>
/// Kiểm tra hợp lệ cho <see cref="SubmitContactCommand"/>.
/// Phản chiếu đúng các ràng buộc phía client của app (Contact Form Screen) để đảm bảo
/// dữ liệu nhất quán dù request đến từ nguồn nào: số điện thoại bắt buộc, email đúng định dạng,
/// và số lượng học viên tối thiểu 20 (ngưỡng gói B2B).
/// </summary>
public class SubmitContactCommandValidator : AbstractValidator<SubmitContactCommand>
{
    private const int MinimumLearnersForB2B = 20;

    public SubmitContactCommandValidator()
    {
        RuleFor(x => x.Request.CenterName)
            .NotEmpty().WithMessage("Tên trung tâm không được để trống.");

        RuleFor(x => x.Request.ContactPerson)
            .NotEmpty().WithMessage("Người liên hệ không được để trống.");

        RuleFor(x => x.Request.Phone)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Request.NumberOfLearners)
            .GreaterThanOrEqualTo(MinimumLearnersForB2B)
            .WithMessage($"Số lượng học viên phải từ {MinimumLearnersForB2B} trở lên để áp dụng gói B2B.");
    }
}
