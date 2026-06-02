namespace SignMate.Domain.Entities;

public class LessonProgress
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.NotStarted;
    public DateTime? LastWatchedAt { get; set; }
    public int WatchDurationSeconds { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

public enum LessonStatus { NotStarted, InProgress, Completed }
