using FluentValidation;

namespace SignMate.Application.Features.Practice.Commands.ReportResult;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu báo kết quả: phiên hợp lệ, điểm tổng nằm trong khoảng [0, 1].
/// </summary>
public class ReportResultCommandValidator : AbstractValidator<ReportResultCommand>
{
    public ReportResultCommandValidator()
    {
        RuleFor(x => x.Request.SessionId)
            .GreaterThan(0).WithMessage("SessionId không hợp lệ.");

        RuleFor(x => x.Request.OverallScore)
            .InclusiveBetween(0f, 1f).WithMessage("Điểm tổng phải nằm trong khoảng 0 đến 1.");
    }
}
