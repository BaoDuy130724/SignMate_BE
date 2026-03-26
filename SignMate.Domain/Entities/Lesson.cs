namespace SignMate.Domain.Entities;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Topic { get; set; }
    public int OrderIndex { get; set; }
    public string? VideoUrl { get; set; }
    public string? Description { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsPublished { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<Sign> Signs { get; set; } = [];
}
