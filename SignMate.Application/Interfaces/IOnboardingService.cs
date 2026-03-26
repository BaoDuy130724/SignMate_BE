using SignMate.Application.DTOs.Onboarding;

namespace SignMate.Application.Interfaces;

public interface IOnboardingService
{
    Task<OnboardingResponse> SubmitOnboardingAsync(Guid userId, OnboardingRequest request);
}
