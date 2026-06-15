using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Features.Subscription.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.Subscribe;

/// <summary>
/// Handler cho <see cref="SubscribeCommand"/>:
/// <list type="bullet">
/// <item>Gói Free (giá 0đ): hủy gói active cũ và kích hoạt gói mới ngay lập tức.</item>
/// <item>Gói trả phí: tạo bản ghi chờ thanh toán, gọi PayOS tạo link, trả về paymentUrl.</item>
/// </list>
/// </summary>
public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, SubscribeResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOsService _payOsService;

    public SubscribeCommandHandler(IUnitOfWork unitOfWork, IPayOsService payOsService)
    {
        _unitOfWork = unitOfWork;
        _payOsService = payOsService;
    }

    /// <inheritdoc />
    public async Task<SubscribeResponse> Handle(SubscribeCommand command, CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(command.Request.PlanId)
            ?? throw new NotFoundException(nameof(SubscriptionPlan), command.Request.PlanId);

        // ── Gói miễn phí: kích hoạt ngay ─────────────────────────────
        if (plan.PriceVnd == 0)
        {
            await SubscriptionActivation.DeactivateActiveSubscriptionsAsync(_unitOfWork, command.UserId, cancellationToken);
            await _unitOfWork.Repository<UserSubscription>().AddAsync(new UserSubscription
            {
                UserId = command.UserId,
                PlanId = plan.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
                IsActive = true
            });
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SubscribeResponse { Success = true, Message = "Đã kích hoạt gói miễn phí." };
        }

        // ── Gói trả phí: tạo pending subscription + PayOS link ──────
        // CHÚ Ý: Không xóa các pending subscription cũ. Nếu xóa, người dùng lỡ thanh toán mã QR cũ 
        // thì hệ thống sẽ không tìm thấy để kích hoạt. Việc dọn dẹp rác nên dùng CronJob quét giao dịch > 24h.
        
        // Tạo OrderCode duy nhất dựa trên timestamp (tránh trùng khi re-seed DB,
        // vì PayOS nhớ tất cả order code cũ trên server của họ).
        var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1_000_000_000;

        var pendingSub = new UserSubscription
        {
            UserId = command.UserId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = false,
            PaymentReference = orderCode.ToString()
        };
        await _unitOfWork.Repository<UserSubscription>().AddAsync(pendingSub);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var returnUrl = command.Request.ReturnUrl
            ?? "http://localhost:5173/payment-callback";

        string paymentUrl;
        try
        {
            paymentUrl = await _payOsService.CreatePaymentLinkAsync(new PayOsPaymentRequest
            {
                OrderCode = orderCode,
                Amount = (int)plan.PriceVnd,
                Description = $"SignMate Plan {plan.Id}",
                ReturnUrl = returnUrl,
                CancelUrl = returnUrl + "?cancel=true"
            });
        }
        catch (Exception ex)
        {
            // Xóa pending subscription nếu PayOS thất bại
            _unitOfWork.Repository<UserSubscription>().Delete(pendingSub);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new BadRequestException(
                !string.IsNullOrWhiteSpace(ex.Message)
                    ? $"Lỗi thanh toán: {ex.Message}"
                    : "Không thể tạo đơn thanh toán. Vui lòng thử lại sau.");
        }

        return new SubscribeResponse
        {
            Success = true,
            Message = "Chuyển hướng tới PayOS.",
            PaymentUrl = paymentUrl,
            SubscriptionId = pendingSub.Id
        };
    }
}
