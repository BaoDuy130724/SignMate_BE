using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Commands.Subscribe;

/// <summary>
/// Lệnh đăng ký một gói cước. Gói Free được kích hoạt ngay; gói trả phí tạo bản ghi chờ thanh toán
/// và trả về link VNPay — <c>POST /api/subscription/subscribe</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
/// <param name="Request">Gói cần đăng ký và URL trả về tùy chọn.</param>
/// <param name="IpAddress">IP client, bắt buộc cho chữ ký giao dịch VNPay.</param>
public record SubscribeCommand(int UserId, SubscribeRequest Request, string IpAddress)
    : ICommand<SubscribeResponse>;
