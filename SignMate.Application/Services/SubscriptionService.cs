using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPay;

    public SubscriptionService(IUnitOfWork unitOfWork, IVnPayService vnPay)
    {
        _unitOfWork = unitOfWork;
        _vnPay = vnPay;
    }

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
    {
        return await _unitOfWork.Repository<SubscriptionPlan>().Query()
            .AsNoTracking()
            .Select(p => new SubscriptionPlanDto
            {
                Id = p.Id, Name = p.Name, PriceVnd = p.PriceVnd,
                DurationDays = p.DurationDays, Type = p.Type.ToString(),
                FeaturesJson = p.FeaturesJson
            }).ToListAsync();
    }

    public async Task<SubscribeResponse> SubscribeAsync(int userId, SubscribeRequest request, string ipAddress)
    {
        var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(request.PlanId);
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
            Id = 0,
            UserId = userId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = false // Pending payment
        };
        await _unitOfWork.Repository<UserSubscription>().AddAsync(sub);
        await _unitOfWork.SaveChangesAsync();

        var returnUrl = request.ReturnUrl ?? "http://localhost:5184/api/subscription/vnpay-return";

        var paymentUrl = _vnPay.CreatePaymentUrl(new VnPayPaymentRequest
        {
            OrderId = sub.Id,
            Amount = plan.PriceVnd,
            OrderInfo = $"SignMate - Thanh toan hoa don {sub.Id.ToString("N")[..8]}",
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

    public async Task<bool> ConfirmPaymentAsync(int subscriptionId)
    {
        var sub = await _unitOfWork.Repository<UserSubscription>().Query()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (sub == null || sub.IsActive) return false;

        // Deactivate other subs for this user
        var others = await _unitOfWork.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == sub.UserId && s.IsActive)
            .ToListAsync();
        foreach (var oth in others) oth.IsActive = false;

        // Activate the paid subscription
        sub.IsActive = true;
        sub.StartDate = DateTime.UtcNow;

        var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(sub.PlanId);
        sub.EndDate = DateTime.UtcNow.AddDays(plan?.DurationDays ?? 30);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<MySubscriptionDto?> GetMySubscriptionAsync(int userId)
    {
        return await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate >= DateTime.UtcNow)
            .Select(s => new MySubscriptionDto
            {
                PlanId = s.PlanId, PlanName = s.Plan.Name, PlanType = s.Plan.Type.ToString(),
                StartDate = s.StartDate, EndDate = s.EndDate, IsActive = s.IsActive
            }).FirstOrDefaultAsync();
    }

    public async Task<bool> HasAccessToProFeaturesAsync(int userId)
    {
        var sub = await _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.IsActive && s.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        return sub != null && (sub.Plan.Type == PlanType.Pro || sub.Plan.Type == PlanType.B2B);
    }

    public async Task<List<SubscriptionListItemDto>> GetAllSubscriptionsAsync()
    {
        return await _unitOfWork.Repository<UserSubscription>().Query()
            .Include(s => s.User)
                .ThenInclude(u => u.Center)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SubscriptionListItemDto
            {
                Id = s.Id,
                UserFullName = s.User.FullName,
                Email = s.User.Email,
                CenterName = s.User.Center != null ? s.User.Center.Name : null,
                PlanName = s.Plan.Name,
                PriceVnd = s.Plan.PriceVnd,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive
            }).ToListAsync();
    }

    private async Task ActivateSubscription(int userId, SubscriptionPlan plan)
    {
        var others = await _unitOfWork.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
        foreach (var oth in others) oth.IsActive = false;

        await _unitOfWork.Repository<UserSubscription>().AddAsync(new UserSubscription
        {
            Id = 0, UserId = userId, PlanId = plan.Id,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
    }
}
