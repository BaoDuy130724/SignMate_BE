namespace SignMate.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public CourseLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Phạm vi nội dung: null = khóa học global (do SuperAdmin/nền tảng quản lý, mọi học viên thấy);
    /// có giá trị = khóa học riêng của một trung tâm (chỉ học viên thuộc center đó mới thấy/học).
    /// </summary>
    public int? CenterId { get; set; }

    public Center? Center { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public enum CourseLevel { Beginner, Intermediate, Advanced }
