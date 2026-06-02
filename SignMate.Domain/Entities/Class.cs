namespace SignMate.Domain.Entities;

public class Class
{
    public int Id { get; set; }
    public int CenterId { get; set; }
    public string Name { get; set; } = null!;
    public int TeacherId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Center Center { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public ICollection<ClassStudent> ClassStudents { get; set; } = [];
    public ICollection<LessonAssignment> LessonAssignments { get; set; } = [];
}
