namespace SignMate.Domain.Entities;

public class Center
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int MaxSeats { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = []; // Includes CenterAdmins, Teachers, Students
    public ICollection<Class> Classes { get; set; } = [];
}
