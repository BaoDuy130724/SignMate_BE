namespace SignMate.Domain.Entities;

public class Sign
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Word { get; set; } = null!;
    public string VideoUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string? ReferenceKeypointData { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public ICollection<SignProgress> SignProgresses { get; set; } = [];
}
