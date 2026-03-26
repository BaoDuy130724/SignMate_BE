namespace SignMate.Application.DTOs.Onboarding;

public class OnboardingRequest
{
    public string Goal { get; set; } = null!;
    public string Level { get; set; } = null!;
}

public class OnboardingResponse
{
    public Guid UserId { get; set; }
    public bool Success { get; set; }
}
