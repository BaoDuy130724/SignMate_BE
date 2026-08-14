namespace SignMate.Application.Interfaces;

/// <summary>
/// Abstraction cho cổng thanh toán PayOS.
/// </summary>
public interface IPayOsService
{
    /// <summary>
    /// Tạo link thanh toán PayOS cho một gói cước.
    /// </summary>
    /// <param name="request">Thông tin đơn hàng.</param>
    /// <returns>URL checkout để redirect người dùng.</returns>
    Task<string> CreatePaymentLinkAsync(PayOsPaymentRequest request);

    /// <summary>
    Task<PayOsWebhookResult> VerifyWebhookAsync(string webhookBody);

    /// <summary>
    /// Kiểm tra trạng thái giao dịch trực tiếp từ PayOS (hữu ích khi webhook bị trễ hoặc test ở localhost).
    /// </summary>
    Task<bool> VerifyPaymentLinkAsync(long orderCode);

    /// <summary>
    /// Lấy chi tiết thông tin thanh toán từ PayOS qua orderCode.
    /// </summary>
    Task<PayOsPaymentInfo?> GetPaymentLinkInformationAsync(long orderCode);
}

public class PayOsPaymentRequest
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string CancelUrl { get; set; } = null!;
}

public class PayOsWebhookResult
{
    public bool IsValid { get; set; }
    public bool IsSuccess { get; set; }
    public long OrderCode { get; set; }
}

public class PayOsPaymentInfo
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public int AmountPaid { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public string? CancellationReason { get; set; }
}
