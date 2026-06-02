using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Interfaces;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetPlansAsync();
    Task<SubscribeResponse> SubscribeAsync(int userId, SubscribeRequest request, string ipAddress);
    Task<bool> ConfirmPaymentAsync(int subscriptionId);
    Task<MySubscriptionDto?> GetMySubscriptionAsync(int userId);
    Task<bool> HasAccessToProFeaturesAsync(int userId);
    Task<List<SubscriptionListItemDto>> GetAllSubscriptionsAsync();
}
