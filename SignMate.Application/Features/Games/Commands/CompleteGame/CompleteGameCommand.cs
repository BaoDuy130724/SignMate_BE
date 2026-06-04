using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Game;

namespace SignMate.Application.Features.Games.Commands.CompleteGame;

/// <summary>
/// Lệnh hoàn tất một lượt chơi: cộng XP cho người chơi và cập nhật streak — <c>POST /api/games/complete</c>.
/// </summary>
/// <param name="UserId">Id người chơi lấy từ JWT.</param>
/// <param name="Request">Phiên chơi và điểm số đạt được (quy đổi thành XP).</param>
public record CompleteGameCommand(int UserId, CompleteGameRequest Request) : ICommand<GameResultResponse>;
