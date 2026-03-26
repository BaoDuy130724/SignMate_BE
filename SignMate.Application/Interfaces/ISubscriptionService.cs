using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Interfaces;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetPlansAsync();
    Task<SubscribeResponse> SubscribeAsync(Guid userId, SubscribeRequest request);
    Task<MySubscriptionDto?> GetMySubscriptionAsync(Guid userId);
    Task<bool> HasAccessToProFeaturesAsync(Guid userId);
}
