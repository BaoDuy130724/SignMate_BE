namespace SignMate.Application.DTOs.Teacher;

public class TeacherCommentDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public int StudentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public int StudentId { get; set; }
    public string Content { get; set; } = null!;
}

public class TeacherDashboardDto
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
}
