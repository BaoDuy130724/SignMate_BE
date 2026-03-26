using SignMate.Application.DTOs.Game;

namespace SignMate.Application.Interfaces;

public interface IGameService
{
    Task<Guid> StartGameAsync(Guid userId, StartGameRequest request);
    Task<GameResultResponse> CompleteGameAsync(Guid userId, CompleteGameRequest request);
}
