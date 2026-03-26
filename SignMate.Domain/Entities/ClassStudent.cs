namespace SignMate.Domain.Entities;

public class ClassStudent
{
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }

    public Class Class { get; set; } = null!;
    public User Student { get; set; } = null!;
}
