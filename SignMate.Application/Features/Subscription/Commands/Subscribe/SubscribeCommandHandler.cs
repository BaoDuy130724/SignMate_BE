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
/// <item>Gói trả phí: tạo bản ghi <see cref="UserSubscription"/> ở trạng thái chờ (IsActive=false)
/// và sinh link thanh toán VNPay; gói chỉ được kích hoạt khi callback thanh toán thành công.</item>
/// </list>
/// </summary>
public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, SubscribeResponse>
{
    private const string DefaultReturnUrl = "http://localhost:5184/api/subscription/vnpay-return";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;

    public SubscribeCommandHandler(IUnitOfWork unitOfWork, IVnPayService vnPayService)
    {
        _unitOfWork = unitOfWork;
        _vnPayService = vnPayService;
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

        // ── Gói trả phí: tạo bản ghi chờ + link VNPay ────────────────
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
    }
}
