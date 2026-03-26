namespace SignMate.Application.DTOs.Game;

public class StartGameRequest
{
    public string GameType { get; set; } = null!; // "SignMatch", "GestureSpeed", etc.
}

public class CompleteGameRequest
{
    public Guid SessionId { get; set; }
    public int Score { get; set; }
}

public class GameResultResponse
{
    public int XpEarned { get; set; }
    public int TotalXp { get; set; }
    public int StreakUpdated { get; set; }
}
