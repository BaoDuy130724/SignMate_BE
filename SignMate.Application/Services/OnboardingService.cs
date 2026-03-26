using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly ISignMateDbContext _db;

    public OnboardingService(ISignMateDbContext db) => _db = db;

    public async Task<OnboardingResponse> SubmitOnboardingAsync(Guid userId, OnboardingRequest request)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return new OnboardingResponse { Success = false };

        user.Goal = request.Goal;
        user.Level = request.Level;
        user.IsOnboarded = true;
        
        await _db.SaveChangesAsync();
        return new OnboardingResponse { UserId = userId, Success = true };
    }
}
