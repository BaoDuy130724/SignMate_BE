using SignMate.Application.DTOs.Game;

namespace SignMate.Application.Interfaces;

public interface IGameService
{
    Task<int> StartGameAsync(int userId, StartGameRequest request);
    Task<GameResultResponse> CompleteGameAsync(int userId, CompleteGameRequest request);
}
