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
/// <item>Gói trả phí: <b>tạm vô hiệu hóa</b> — cổng VNPay sandbox đã được gỡ, sẽ thay bằng PayOS.
/// Hiện trả về 503 (đang bảo trì). Logic VNPay cũ được giữ lại dạng comment bên dưới để tham chiếu.</item>
/// </list>
/// </summary>
public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, SubscribeResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        // ── Gói trả phí: TẠM VÔ HIỆU HÓA (chờ tích hợp PayOS) ─────────
        throw new ServiceUnavailableException(
            "Thanh toán gói trả phí đang tạm bảo trì để nâng cấp cổng thanh toán. Vui lòng thử lại sau.");

        /* === VNPay (giữ lại để tham chiếu khi làm PayOS) =============
        const string DefaultReturnUrl = "http://localhost:5184/api/subscription/vnpay-return";

        var pendingSub = new UserSubscription
        {
            UserId = command.UserId,
            PlanId = plan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            IsActive = false
        };
        await _unitOfWork.Repository<UserSubscription>().AddAsync(pendingSub);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var returnUrl = command.Request.ReturnUrl ?? DefaultReturnUrl;
        var paymentUrl = _vnPayService.CreatePaymentUrl(new VnPayPaymentRequest
        {
            OrderId = pendingSub.Id,
            Amount = plan.PriceVnd,
            OrderInfo = $"SignMate - Thanh toan hoa don {pendingSub.Id.ToString("N")[..8]}",
            ReturnUrl = returnUrl
        }, command.IpAddress);

        return new SubscribeResponse
        {
            Success = true,
            Message = "Chuyển hướng tới VNPay.",
            PaymentUrl = paymentUrl,
            SubscriptionId = pendingSub.Id
        };
        ============================================================= */
    }
}
