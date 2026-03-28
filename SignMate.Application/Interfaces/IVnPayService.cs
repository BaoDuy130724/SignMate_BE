namespace SignMate.Application.Interfaces;

public interface IVnPayService
{
    /// <summary>
    /// Create a VNPay payment URL for a subscription.
    /// </summary>
    string CreatePaymentUrl(VnPayPaymentRequest request, string ipAddress);

    /// <summary>
    /// Validate VNPay callback parameters (return URL or IPN).
    /// </summary>
    VnPayCallbackResult ValidateCallback(IDictionary<string, string> vnpayParams);
}

public class VnPayPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
}

public class VnPayCallbackResult
{
    public bool IsValid { get; set; }
    public bool IsSuccess { get; set; }
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
}
