using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISignMateDbContext _db;

    public SubscriptionService(ISignMateDbContext db) => _db = db;

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
    {
        return await _db.SubscriptionPlans
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id, Name = p.Name, PriceVnd = p.PriceVnd,
                DurationDays = p.DurationDays, Type = p.Type.ToString(),
                FeaturesJson = p.FeaturesJson
            }).ToListAsync();
    }

    public async Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest request)
    {
        var plan = await _db.SubscriptionPlans.FindAsync(request.PlanId);
        if (plan == null) return new SubscribeResponse { Success = false, Message = "Plan not found" };

        var sub = new UserSubscription
        {
            Id = Guid.NewGuid(), UserId = userId, PlanId = plan.Id,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = true
        };
        _db.UserSubscriptions.Add(sub);
        
        // Deactivate others
        var others = await _db.UserSubscriptions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        foreach (var oth in others) oth.IsActive = false;
        
        await _db.SaveChangesAsync();

        return new SubscribeResponse { Success = true, Message = "Mock payment logic completed" };
    }

    public async Task<MySubscriptionDto?> GetMySubscriptionAsync(Guid userId)
    {
        return await _db.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .Select(s => new MySubscriptionDto
            {
                PlanId = s.PlanId, PlanName = s.Plan.Name, PlanType = s.Plan.Type.ToString(),
                StartDate = s.StartDate, EndDate = s.EndDate, IsActive = s.IsActive
            }).FirstOrDefaultAsync();
    }

    public async Task<bool> HasAccessToProFeaturesAsync(Guid userId)
    {
        var sub = await _db.UserSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
            
        return sub != null && (sub.Plan.Type == PlanType.Pro || sub.Plan.Type == PlanType.B2B);
    }
}
