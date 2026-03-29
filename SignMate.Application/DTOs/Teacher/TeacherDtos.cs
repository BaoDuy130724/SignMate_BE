namespace SignMate.Application.DTOs.Teacher;

public class TeacherCommentDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public Guid StudentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public Guid StudentId { get; set; }
    public string Content { get; set; } = null!;
}

public class TeacherDashboardDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
}
