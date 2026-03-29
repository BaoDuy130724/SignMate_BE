namespace SignMate.Domain.Entities;

public enum ReferenceRequestStatus
{
    Pending = 0,
    ReadyForReview = 1,
    Approved = 2,
    Rejected = 3
}

public class SignReferenceRequest
{
    public Guid Id { get; set; }
    
    public Guid SignId { get; set; }
    public Sign Sign { get; set; } = null!;
    
    public Guid UploaderId { get; set; }
    public User Uploader { get; set; } = null!;
    
    public string VideoUrl { get; set; } = null!;
    public string? ExtractedKeypoints { get; set; }
    
    public ReferenceRequestStatus Status { get; set; } = ReferenceRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }
}
