using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using SignMate.Application.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace SignMate.Infrastructure.ExternalServices;

public class PayOsService : IPayOsService
{
    private readonly PayOSClient _payOS;
    private readonly string _clientId;
    private readonly string _apiKey;

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

        _clientId = clientId;
        _apiKey = apiKey;

        _payOS = new PayOSClient(new PayOSOptions
        {
            ClientId = clientId,
            ApiKey = apiKey,
            ChecksumKey = checksumKey,
            // Ép kết nối IPv4 tới PayOS. .NET 8 chưa có "Happy Eyeballs": nếu host có bản ghi
            // IPv6 (PayOS qua Cloudflare có) nhưng mạng IPv6 hỏng, runtime thử IPv6 trước rồi
            // treo ~21s mới fallback IPv4 → tạo link mất ~40s (client timeout). Buộc IPv4 để
            // luôn nhanh & ổn định trên mọi môi trường.
            HttpClient = new HttpClient(new SocketsHttpHandler
            {
                ConnectTimeout = TimeSpan.FromSeconds(15),
                ConnectCallback = async (context, ct) =>
                {
                    var addresses = await Dns.GetHostAddressesAsync(
                        context.DnsEndPoint.Host, AddressFamily.InterNetwork, ct);
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };
                    try
                    {
                        await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, ct);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            })
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
    public async Task<bool> VerifyPaymentLinkAsync(long orderCode)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-client-id", _clientId);
            client.DefaultRequestHeaders.Add("x-api-key", _apiKey);

            var response = await client.GetAsync($"https://api-merchant.payos.vn/v2/payment-requests/{orderCode}");
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("data", out var dataEl) && 
                dataEl.TryGetProperty("status", out var statusEl))
            {
                return statusEl.GetString() == "PAID";
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
