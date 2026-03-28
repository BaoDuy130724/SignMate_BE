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
    public string? ReturnUrl { get; set; }
}

public class SubscribeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string? PaymentUrl { get; set; }
    public Guid? SubscriptionId { get; set; }
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

public class SubscriptionListItemDto
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? CenterName { get; set; } // If part of a center
    public string PlanName { get; set; } = null!;
    public decimal PriceVnd { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
