using System.ComponentModel.DataAnnotations;

namespace SignMate.Application.DTOs.Course;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Level { get; set; } = null!;
    public bool IsPublished { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LessonCount { get; set; }
}

public class CourseDetailDto : CourseDto
{
    public List<LessonDto> Lessons { get; set; } = [];
}

public class CreateCourseRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }

    [Required]
    public string Level { get; set; } = null!;
}

public class UpdateCourseRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Level { get; set; }
    public bool? IsPublished { get; set; }
}

public class LessonDto
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
    public int SignCount { get; set; }
}

public class LessonDetailDto : LessonDto
{
    public List<SignDto> Signs { get; set; } = [];
}

public class SignDto
{
    public Guid Id { get; set; }
    public string Word { get; set; } = null!;
    public string VideoUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
}
