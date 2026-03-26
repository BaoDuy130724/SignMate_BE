namespace SignMate.Application.DTOs.B2BContact;

public class CreateB2BContactRequest
{
    public string CenterName { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int NumberOfLearners { get; set; }
}

public class B2BContactLeadDto
{
    public Guid Id { get; set; }
    public string CenterName { get; set; } = null!;
    public string ContactPerson { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int NumberOfLearners { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
