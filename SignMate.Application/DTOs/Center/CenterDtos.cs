namespace SignMate.Application.DTOs.Center;

public class CenterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int MaxSeats { get; set; }
    public bool IsActive { get; set; }
}

public class CenterDashboardDto
{
    public int TotalStudents { get; set; }
    public int ActiveLearners { get; set; }
    public double AverageAccuracy { get; set; }
}
