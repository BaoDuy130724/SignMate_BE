using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Games.Commands.StartGame;

/// <summary>
/// Lệnh bắt đầu một lượt chơi minigame — <c>POST /api/games/start</c>. Trả về Id phiên chơi.
/// </summary>
/// <param name="UserId">Id người chơi lấy từ JWT.</param>
/// <param name="GameType">Loại game (ví dụ: "SignMatch", "GestureSpeed").</param>
public record StartGameCommand(int UserId, string GameType) : ICommand<int>;
