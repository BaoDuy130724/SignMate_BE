namespace SignMate.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal PriceVnd { get; set; }
    public int DurationDays { get; set; }
    public string Type { get; set; } = null!;
    public string? FeaturesJson { get; set; }
}

public class SubscribeRequest
{
    public Guid PlanId { get; set; }
    public string PaymentMethod { get; set; } = "Momo"; // Mocked
}

public class SubscribeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? PaymentUrl { get; set; }
}

public class MySubscriptionDto
{
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = null!;
    public string PlanType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
