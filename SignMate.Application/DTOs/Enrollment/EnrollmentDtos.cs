using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Enrollment;

public class EnrollRequest
{
    [Required]
    public int CourseId { get; set; }
}

public class EnrollmentResultDto
{
    public int EnrollmentId { get; set; }
}

public class EnrollmentDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string? CourseThumbnailUrl { get; set; }
    public string CourseLevel { get; set; } = null!;
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
}
