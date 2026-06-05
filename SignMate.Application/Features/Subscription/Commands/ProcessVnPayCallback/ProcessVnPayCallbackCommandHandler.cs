// VNPay tạm vô hiệu hóa (sẽ thay bằng PayOS). Handler bị loại khỏi biên dịch để DI không
// yêu cầu IVnPayService lúc startup. Giữ nguyên code để tham chiếu khi tích hợp cổng mới.
#if false
using MediatR;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Features.Subscription.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Commands.ProcessVnPayCallback;

/// <summary>
/// Handler cho <see cref="ProcessVnPayCallbackCommand"/>: xác thực chữ ký VNPay rồi (nếu hợp lệ và
/// thành công) kích hoạt gói tương ứng với mã giao dịch <c>vnp_TxnRef</c>.
/// Idempotent — gọi lặp lại (return URL + IPN cùng trỏ về một giao dịch) sẽ không kích hoạt hai lần
/// vì chỉ xử lý khi gói còn ở trạng thái chờ.
/// </summary>
public class ProcessVnPayCallbackCommandHandler
    : IRequestHandler<ProcessVnPayCallbackCommand, VnPayCallbackOutcomeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;

    public ProcessVnPayCallbackCommandHandler(IUnitOfWork unitOfWork, IVnPayService vnPayService)
    {
        _unitOfWork = unitOfWork;
        _vnPayService = vnPayService;
    }

    /// <inheritdoc />
    public async Task<VnPayCallbackOutcomeDto> Handle(
        ProcessVnPayCallbackCommand command, CancellationToken cancellationToken)
    {
        var validation = _vnPayService.ValidateCallback(command.Parameters.ToDictionary(p => p.Key, p => p.Value));

        if (validation.IsValid && validation.IsSuccess
            && command.Parameters.TryGetValue("vnp_TxnRef", out var txnRef)
            && int.TryParse(txnRef, out var subscriptionId))
        {
            await ConfirmPaymentAsync(subscriptionId, cancellationToken);
        }

        return new VnPayCallbackOutcomeDto { IsValid = validation.IsValid, IsSuccess = validation.IsSuccess };
    }

    /// <summary>
    /// Kích hoạt gói trả phí đã thanh toán: hủy gói active cũ và bật gói mới cùng thời hạn theo plan.
    /// Toàn bộ thay đổi nằm trong một SaveChanges nên đảm bảo Atomicity.
    /// </summary>
    private async Task ConfirmPaymentAsync(int subscriptionId, CancellationToken cancellationToken)
    {
        var sub = await _unitOfWork.Repository<UserSubscription>().GetByIdAsync(subscriptionId);
        if (sub is null || sub.IsActive)
            return; // Không tồn tại hoặc đã kích hoạt → bỏ qua (idempotent).

        await SubscriptionActivation.DeactivateActiveSubscriptionsAsync(_unitOfWork, sub.UserId, cancellationToken);

        var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(sub.PlanId);
        sub.IsActive = true;
        sub.StartDate = DateTime.UtcNow;
        sub.EndDate = DateTime.UtcNow.AddDays(plan?.DurationDays ?? 30);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
#endif
