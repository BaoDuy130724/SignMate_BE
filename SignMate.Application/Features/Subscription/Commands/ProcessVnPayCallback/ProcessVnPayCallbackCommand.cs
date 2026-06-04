using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Commands.ProcessVnPayCallback;

/// <summary>
/// Lệnh xử lý callback từ VNPay (dùng chung cho cả return URL và IPN): xác thực chữ ký và
/// kích hoạt gói nếu thanh toán thành công — <c>GET /api/subscription/vnpay-return</c> &amp;
/// <c>/vnpay-ipn</c>.
/// </summary>
/// <param name="Parameters">Toàn bộ query param VNPay gửi về để xác thực chữ ký.</param>
public record ProcessVnPayCallbackCommand(IReadOnlyDictionary<string, string> Parameters)
    : ICommand<VnPayCallbackOutcomeDto>;
