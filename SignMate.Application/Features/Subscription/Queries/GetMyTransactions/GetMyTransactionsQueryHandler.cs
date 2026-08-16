using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Features.Subscription.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Queries.GetMyTransactions;

/// <summary>
/// Handler lấy danh sách lịch sử giao dịch của người dùng hiện tại và đồng bộ trạng thái từ PayOS.
/// </summary>
public class GetMyTransactionsQueryHandler : IRequestHandler<GetMyTransactionsQuery, List<TransactionHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOsService _payOsService;

    public GetMyTransactionsQueryHandler(IUnitOfWork unitOfWork, IPayOsService payOsService)
    {
        _unitOfWork = unitOfWork;
        _payOsService = payOsService;
    }

    /// <inheritdoc />
    public async Task<List<TransactionHistoryDto>> Handle(GetMyTransactionsQuery query, CancellationToken cancellationToken)
    {
        // Kiểm tra người dùng: Chỉ học viên B2C (không thuộc trung tâm - CenterId == null) mới có lịch sử giao dịch cá nhân
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(query.UserId);
        if (user == null || user.CenterId != null)
        {
            return new List<TransactionHistoryDto>();
        }

        var subscriptionQuery = _unitOfWork.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.UserId == query.UserId)
            .AsQueryable();

        // Lọc theo thời gian: giới hạn tối đa khoảng 1 tháng (31 ngày) để tránh query quá nặng
        var now = DateTime.UtcNow;
        var toDate = query.ToDate ?? now;
        DateTime? fromDate = query.FromDate;

        if (fromDate.HasValue)
        {
            // Nếu khoảng cách lớn hơn 31 ngày, tự động giới hạn về 31 ngày trước toDate
            if ((toDate - fromDate.Value).TotalDays > 31)
            {
                fromDate = toDate.AddDays(-31);
            }
            subscriptionQuery = subscriptionQuery.Where(s => s.StartDate >= fromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            subscriptionQuery = subscriptionQuery.Where(s => s.StartDate <= query.ToDate.Value);
        }

        var subscriptions = await subscriptionQuery
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);

        var result = new List<TransactionHistoryDto>();
        var dbChanged = false;

        foreach (var sub in subscriptions)
        {
            long? orderCode = null;
            if (!string.IsNullOrWhiteSpace(sub.PaymentReference) && long.TryParse(sub.PaymentReference, out var parsedCode))
            {
                orderCode = parsedCode;
            }

            string status;

            if (sub.Plan.PriceVnd == 0)
            {
                status = "FREE";
            }
            else if (sub.IsActive)
            {
                status = "PAID";
            }
            else if (orderCode.HasValue)
            {
                // Nếu giao dịch tạo quá 24h và chưa active thì coi như hết hạn
                if (sub.StartDate < DateTime.UtcNow.AddHours(-24))
                {
                    status = "EXPIRED";
                }
                else
                {
                    // Giao dịch gần đây: tra cứu trạng thái thời gian thực từ PayOS
                    var payOsInfo = await _payOsService.GetPaymentLinkInformationAsync(orderCode.Value);
                    if (payOsInfo != null)
                    {
                        if (payOsInfo.Status == "PAID")
                        {
                            // Tự động kích hoạt gói nếu người dùng đã thanh toán thành công nhưng webhook bị trễ
                            await SubscriptionActivation.DeactivateActiveSubscriptionsAsync(
                                _unitOfWork, query.UserId, cancellationToken);

                            sub.IsActive = true;
                            sub.StartDate = DateTime.UtcNow;
                            sub.EndDate = DateTime.UtcNow.AddDays(sub.Plan.DurationDays);
                            dbChanged = true;
                            status = "PAID";
                        }
                        else if (payOsInfo.Status == "CANCELLED")
                        {
                            status = "CANCELLED";
                        }
                        else if (payOsInfo.Status == "EXPIRED")
                        {
                            status = "EXPIRED";
                        }
                        else
                        {
                            status = "PENDING";
                        }
                    }
                    else
                    {
                        status = "PENDING";
                    }
                }
            }
            else
            {
                status = "INACTIVE";
            }

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                !string.Equals(status, query.Status, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(new TransactionHistoryDto
            {
                Id = sub.Id,
                OrderCode = orderCode,
                PlanId = sub.PlanId,
                PlanName = sub.Plan.Name,
                PlanType = sub.Plan.Type.ToString(),
                PriceVnd = sub.Plan.PriceVnd,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                IsActive = sub.IsActive,
                Status = status
            });
        }

        if (dbChanged)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
