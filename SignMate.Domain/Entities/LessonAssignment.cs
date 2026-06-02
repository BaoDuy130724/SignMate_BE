namespace SignMate.Domain.Entities;

public class LessonAssignment
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public int LessonId { get; set; }
    public int AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? DueDate { get; set; }

    public Class Class { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}
