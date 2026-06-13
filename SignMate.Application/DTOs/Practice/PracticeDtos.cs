using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Practice;

public class StartSessionRequest
{
    [Required]
    public int SignId { get; set; }
}

public class StartSessionResponse
{
    public int SessionId { get; set; }
}

public class EndSessionRequest
{
    [Required]
    public int SessionId { get; set; }
}

public class AttemptResponse
{
    public int AttemptId { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
    public string? GeminiFeedback { get; set; }
}

public class FeedbackDto
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}

public class PracticeHistoryDto
{
    public int SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalAttempts { get; set; }
    public List<AttemptSummaryDto> Attempts { get; set; } = [];
}

public class AttemptSummaryDto
{
    public int Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
}
