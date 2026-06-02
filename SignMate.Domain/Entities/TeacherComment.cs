namespace SignMate.Domain.Entities;

public class TeacherComment
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public User Teacher { get; set; } = null!;
    public User Student { get; set; } = null!;
}
