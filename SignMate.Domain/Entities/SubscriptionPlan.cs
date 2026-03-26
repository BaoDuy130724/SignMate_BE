namespace SignMate.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal PriceVnd { get; set; }
    public int DurationDays { get; set; }
    public PlanType Type { get; set; }
    public string? FeaturesJson { get; set; }
    
    public ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
}

public enum PlanType { Free, Basic, Pro, B2B }
