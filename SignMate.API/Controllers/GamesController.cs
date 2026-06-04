using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Game;
using SignMate.Application.Features.Games.Commands.CompleteGame;
using SignMate.Application.Features.Games.Commands.StartGame;

namespace SignMate.API.Controllers;

/// <summary>
/// Minigame học ký hiệu: bắt đầu lượt chơi và hoàn tất để nhận XP, cập nhật streak.
/// </summary>
[Route("api/games")]
[Authorize]
public class GamesController : BaseApiController
{
    /// <summary>Bắt đầu một lượt chơi. <c>POST /api/games/start</c>.</summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessionId = await Mediator.Send(new StartGameCommand(userId, request.GameType));
        return Success(new { sessionId });
    }

    /// <summary>Hoàn tất lượt chơi và nhận kết quả XP/streak. <c>POST /api/games/complete</c>.</summary>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteGame([FromBody] CompleteGameRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new CompleteGameCommand(userId, request));
        return Success(result);
    }
}
