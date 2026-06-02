namespace SignMate.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = [];
}
