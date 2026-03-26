namespace SignMate.Domain.Entities;

public class Class
{
    public Guid Id { get; set; }
    public Guid CenterId { get; set; }
    public string Name { get; set; } = null!;
    public Guid TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Center Center { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public ICollection<ClassStudent> ClassStudents { get; set; } = [];
    public ICollection<LessonAssignment> LessonAssignments { get; set; } = [];
}
