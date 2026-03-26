namespace SignMate.Domain.Entities;

public class B2BContactLead
{
    public Guid Id { get; set; }
    public string CenterName { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int NumberOfLearners { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.New;
    public DateTime CreatedAt { get; set; }
}

public enum ContactStatus { New, Contacted, Converted, Rejected }
