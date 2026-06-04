using FluentValidation;

namespace SignMate.Application.Features.Games.Commands.StartGame;

/// <summary>
/// Kiểm tra hợp lệ yêu cầu bắt đầu game: loại game không được để trống.
/// </summary>
public class StartGameCommandValidator : AbstractValidator<StartGameCommand>
{
    public StartGameCommandValidator()
    {
        RuleFor(x => x.GameType)
            .NotEmpty().WithMessage("Loại game không được để trống.");
    }
}
