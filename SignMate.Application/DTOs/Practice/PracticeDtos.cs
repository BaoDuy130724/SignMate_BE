using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Practice;

public class StartSessionRequest
{
    [Required]
    public Guid SignId { get; set; }
}

public class StartSessionResponse
{
    public Guid SessionId { get; set; }
}

public class EndSessionRequest
{
    [Required]
    public Guid SessionId { get; set; }
}

public class AttemptResponse
{
    public Guid AttemptId { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
}

public class FeedbackDto
{
    public string Type { get; set; } = null!;
    public float Score { get; set; }
    public string Message { get; set; } = null!;
}

public class PracticeHistoryDto
{
    public Guid SessionId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int TotalAttempts { get; set; }
    public List<AttemptSummaryDto> Attempts { get; set; } = [];
}

public class AttemptSummaryDto
{
    public Guid Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
}

public class ReportResultRequest
{
    [Required]
    public Guid SessionId { get; set; }
    public float OverallScore { get; set; }
    public List<FeedbackDto> Feedbacks { get; set; } = [];
}
