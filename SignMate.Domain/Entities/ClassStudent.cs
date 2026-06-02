namespace SignMate.Domain.Entities;

public class ClassStudent
{
    public int ClassId { get; set; }
    public int StudentId { get; set; }

    public Class Class { get; set; } = null!;
    public User Student { get; set; } = null!;
}
