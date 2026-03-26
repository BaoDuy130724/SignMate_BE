namespace SignMate.Domain.Entities;

public class TeacherComment
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid StudentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public User Teacher { get; set; } = null!;
    public User Student { get; set; } = null!;
}
