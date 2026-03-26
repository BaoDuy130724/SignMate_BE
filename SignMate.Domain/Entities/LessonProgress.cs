namespace SignMate.Domain.Entities;

public class LessonProgress
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.NotStarted;
    public DateTime? LastWatchedAt { get; set; }
    public int WatchDurationSeconds { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

public enum LessonStatus { NotStarted, InProgress, Completed }
