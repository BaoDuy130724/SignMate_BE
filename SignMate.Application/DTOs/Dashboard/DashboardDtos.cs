using SignMate.Application.DTOs.Course;

namespace SignMate.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public double AverageAccuracy { get; set; }
    public int CurrentStreak { get; set; }
    public LessonDto? SuggestedLesson { get; set; }
    public List<DeadlineDto> Deadlines { get; set; } = [];
}

public class DeadlineDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Duedate { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

public class ProgressStatsDto
{
    public double OverallAccuracy { get; set; }
    public int TotalLessonsCompleted { get; set; }
    public int TotalSignsMastered { get; set; }
    public List<string> WeakTopics { get; set; } = [];
    public Dictionary<string, int> AccuracyByTopic { get; set; } = [];
}
