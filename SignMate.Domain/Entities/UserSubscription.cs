namespace SignMate.Domain.Entities;

public class UserSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? PaymentReference { get; set; }

    public User User { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
