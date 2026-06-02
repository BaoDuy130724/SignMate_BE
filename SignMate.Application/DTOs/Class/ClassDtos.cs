using SignMate.Application.DTOs.User;

namespace SignMate.Application.DTOs.Class;

public class ClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public int StudentCount { get; set; }
    public string Schedule { get; set; } = "";
    public string Status { get; set; } = "Active";
    public List<ClassStudentDetailDto> Students { get; set; } = [];
}

/// <summary>
/// Student detail nested inside ClassDto — matches Mobile's ClassModel which expects
/// a 'students' list with UserModel fields (id, fullName, email, practiceAccuracy).
/// </summary>
public class ClassStudentDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int PracticeAccuracy { get; set; }
}

public class CreateClassRequest
{
    public string Name { get; set; } = null!;
    public int TeacherId { get; set; }
}

public class AddStudentsRequest
{
    public List<int> StudentIds { get; set; } = [];
}

public class AssignLessonRequest
{
    public int LessonId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ClassStudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
