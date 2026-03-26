namespace SignMate.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public CourseLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public enum CourseLevel { Beginner, Intermediate, Advanced }
