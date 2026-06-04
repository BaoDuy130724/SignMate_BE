using FluentValidation;

namespace SignMate.Application.Features.Games.Commands.CompleteGame;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu hoàn tất game: phiên hợp lệ, điểm số không âm.
/// </summary>
public class CompleteGameCommandValidator : AbstractValidator<CompleteGameCommand>
{
    public CompleteGameCommandValidator()
    {
        RuleFor(x => x.Request.SessionId)
            .GreaterThan(0).WithMessage("SessionId không hợp lệ.");

        RuleFor(x => x.Request.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Điểm số không được âm.");
    }
}
