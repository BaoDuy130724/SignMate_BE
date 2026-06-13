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

        var returnUrl = command.Request.ReturnUrl
            ?? "http://localhost:5173/payment-callback";

        var paymentUrl = await _payOsService.CreatePaymentLinkAsync(new PayOsPaymentRequest
        {
            OrderCode = pendingSub.Id,
            Amount = (int)plan.PriceVnd,
            Description = $"SignMate Plan {plan.Id}",
            ReturnUrl = returnUrl,
            CancelUrl = returnUrl + "?cancel=true"
        });

        return new SubscribeResponse
        {
            Success = true,
            Message = "Chuyển hướng tới PayOS.",
            PaymentUrl = paymentUrl,
            SubscriptionId = pendingSub.Id
        };
    }
}
