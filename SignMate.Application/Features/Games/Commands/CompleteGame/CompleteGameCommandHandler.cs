using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Game;
using SignMate.Application.Features.Streaks.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Games.Commands.CompleteGame;

/// <summary>
/// Handler cho <see cref="CompleteGameCommand"/>: ghi nhận XP cho phiên chơi, cộng dồn XP cho người dùng
/// và cập nhật streak. Ba bước ghi phụ thuộc nhau (phiên + user + streak) gói trong một transaction để
/// đảm bảo Atomicity — XP và streak luôn nhất quán với nhau.
/// </summary>
public class CompleteGameCommandHandler : IRequestHandler<CompleteGameCommand, GameResultResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteGameCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<GameResultResponse> Handle(CompleteGameCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var session = await _unitOfWork.Repository<GameSession>().GetByIdAsync(request.SessionId);
        if (session is null || session.UserId != command.UserId)
            throw new NotFoundException(nameof(GameSession), request.SessionId);

        var user = await _unitOfWork.Repository<User>().GetByIdAsync(command.UserId)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        int currentStreak;
        await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            session.XpEarned = request.Score; // Quy đổi điểm thành XP của lượt chơi.
            user.XpPoints += request.Score;

            var streak = await StreakActivity.RecordAsync(_unitOfWork, command.UserId, cancellationToken);
            currentStreak = streak.CurrentStreak;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return new GameResultResponse
        {
            XpEarned = request.Score,
            TotalXp = user.XpPoints,
            StreakUpdated = currentStreak
        };
    }
}
