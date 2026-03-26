namespace SignMate.Application.DTOs.StudentTracking;

public class StudentTrackingStatsDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public double AccuracyPercent { get; set; }
    public List<string> WeakTopics { get; set; } = [];
    public int PracticeFrequencyDays { get; set; } // practiced X times in last 7 days
}

public class TrackingReportResponse
{
    public string ReportUrl { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
}
