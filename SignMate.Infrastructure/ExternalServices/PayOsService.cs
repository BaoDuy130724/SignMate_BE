using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using SignMate.Application.Interfaces;
using System.Text.Json;

namespace SignMate.Infrastructure.ExternalServices;

public class PayOsService : IPayOsService
{
    private readonly PayOSClient _payOS;

    public PayOsService(IConfiguration config)
    {
        var clientId = config["PayOS:ClientId"];
        var apiKey = config["PayOS:ApiKey"];
        var checksumKey = config["PayOS:ChecksumKey"];

        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Missing or empty PayOS:ClientId in configuration.");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Missing or empty PayOS:ApiKey in configuration.");
        if (string.IsNullOrWhiteSpace(checksumKey))
            throw new ArgumentException("Missing or empty PayOS:ChecksumKey in configuration.");

        _payOS = new PayOSClient(new PayOSOptions
        {
            ClientId = clientId,
            ApiKey = apiKey,
            ChecksumKey = checksumKey
        });
    }

    public async Task<string> CreatePaymentLinkAsync(PayOsPaymentRequest request)
    {
        var items = new List<PaymentLinkItem>
        {
            new() { Name = "Gói cước SignMate", Quantity = 1, Price = request.Amount }
        };

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = request.OrderCode,
            Amount = request.Amount,
            Description = request.Description,
            Items = items,
            CancelUrl = request.CancelUrl,
            ReturnUrl = request.ReturnUrl
        };

        var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);
        return result.CheckoutUrl;
    }

    public async Task<PayOsWebhookResult> VerifyWebhookAsync(string webhookBody)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<Webhook>(webhookBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (webhook == null)
                return new PayOsWebhookResult { IsValid = false };

            var webhookData = await _payOS.Webhooks.VerifyAsync(webhook);

            return new PayOsWebhookResult
            {
                IsValid = true,
                IsSuccess = true,
                OrderCode = webhookData.OrderCode
            };
        }
        catch
        {
            return new PayOsWebhookResult { IsValid = false };
        }
    }
}
