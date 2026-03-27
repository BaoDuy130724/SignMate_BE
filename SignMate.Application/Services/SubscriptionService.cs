using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISignMateDbContext _db;
    private readonly IVnPayService _vnPay;

    public SubscriptionService(ISignMateDbContext db, IVnPayService vnPay)
    {
        _db = db;
        _vnPay = vnPay;
    }

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id, Name = p.Name, PriceVnd = p.PriceVnd,
                DurationDays = p.DurationDays, Type = p.Type.ToString(),
                FeaturesJson = p.FeaturesJson
            }).ToListAsync();
    }

    public async Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest request, string ipAddress)
    {
        var plan = await _db.SubscriptionPlans.FindAsync(request.PlanId);
        if (plan == null)
            return new SubscribeResponse { Success = false, Message = "Plan not found" };

        // Free plan — activate immediately
        if (plan.PriceVnd == 0)
        {
            await ActivateSubscription(userId, plan);
            return new SubscribeResponse { Success = true, Message = "Free plan activated" };
        }

        // Paid plan — create pending subscription + VNPay URL
        var sub = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = false // Pending payment
        };
        _db.UserSubscriptions.Add(sub);
        await _db.SaveChangesAsync();

        var returnUrl = request.ReturnUrl ?? "http://localhost:5184/api/subscription/vnpay-return";

        var paymentUrl = _vnPay.CreatePaymentUrl(new VnPayPaymentRequest
        {
            OrderId = sub.Id,
            Amount = plan.PriceVnd,
            OrderInfo = $"SignMate - Nang cap goi {plan.Name}",
            ReturnUrl = returnUrl
        }, ipAddress);

        return new SubscribeResponse
        {
            Success = true,
            Message = "Redirect to VNPay",
            PaymentUrl = paymentUrl,
            SubscriptionId = sub.Id
        };
    }

    public async Task<bool> ConfirmPaymentAsync(Guid subscriptionId)
    {
        var sub = await _db.UserSubscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (sub == null || sub.IsActive) return false;

        // Deactivate other subs for this user
        var others = await _db.UserSubscriptions
            .Where(s => s.UserId == sub.UserId && s.IsActive)
            .ToListAsync();
        foreach (var oth in others) oth.IsActive = false;

        // Activate the paid subscription
        sub.IsActive = true;
        sub.StartDate = DateTime.UtcNow;
        sub.EndDate = DateTime.UtcNow.AddDays(
            (await _db.SubscriptionPlans.FindAsync(sub.PlanId))?.DurationDays ?? 30
        );

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MySubscriptionDto?> GetMySubscriptionAsync(Guid userId)
    {
        return await _db.UserSubscriptions
            .AsNoTracking()
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
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        return sub != null && (sub.Plan.Type == PlanType.Pro || sub.Plan.Type == PlanType.B2B);
    }

    private async Task ActivateSubscription(Guid userId, SubscriptionPlan plan)
    {
        var others = await _db.UserSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
        foreach (var oth in others) oth.IsActive = false;

        _db.UserSubscriptions.Add(new UserSubscription
        {
            Id = Guid.NewGuid(), UserId = userId, PlanId = plan.Id,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = true
        });

        await _db.SaveChangesAsync();
    }
}
