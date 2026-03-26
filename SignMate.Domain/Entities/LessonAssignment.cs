namespace SignMate.Domain.Entities;

public class LessonAssignment
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid LessonId { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? DueDate { get; set; }

    public Class Class { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}
