using MediatR;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Games.Commands.StartGame;

/// <summary>
/// Handler cho <see cref="StartGameCommand"/>: tạo bản ghi phiên chơi mới (chưa có XP) và trả về Id
/// để client gắn với lượt chơi và gọi hoàn tất sau đó. Ghi đơn nên một lần <c>SaveChanges</c> là đủ.
/// </summary>
public class StartGameCommandHandler : IRequestHandler<StartGameCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartGameCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<int> Handle(StartGameCommand command, CancellationToken cancellationToken)
    {
        var session = new GameSession
        {
            UserId = command.UserId,
            GameType = command.GameType,
            PlayedAt = DateTime.UtcNow,
            XpEarned = 0
        };

        await _unitOfWork.Repository<GameSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}
