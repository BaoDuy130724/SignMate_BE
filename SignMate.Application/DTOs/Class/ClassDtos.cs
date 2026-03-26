using SignMate.Application.DTOs.User;

namespace SignMate.Application.DTOs.Class;

public class ClassDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public int StudentCount { get; set; }
}

public class CreateClassRequest
{
    public string Name { get; set; } = null!;
    public Guid TeacherId { get; set; }
}

public class AddStudentsRequest
{
    public List<Guid> StudentIds { get; set; } = [];
}

public class AssignLessonRequest
{
    public Guid LessonId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ClassStudentDto
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
