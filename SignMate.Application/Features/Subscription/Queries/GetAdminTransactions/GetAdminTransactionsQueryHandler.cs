using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Subscription.Queries.GetAdminTransactions;

/// <summary>
/// Handler lấy danh sách giao dịch cho SuperAdmin / CenterAdmin.
/// </summary>
public class GetAdminTransactionsQueryHandler : IRequestHandler<GetAdminTransactionsQuery, List<AdminTransactionHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminTransactionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<AdminTransactionHistoryDto>> Handle(
        GetAdminTransactionsQuery query, CancellationToken cancellationToken)
    {
        var subscriptionQuery = _unitOfWork.Repository<UserSubscription>().Query()
            .AsNoTracking()
            .Include(s => s.User)
                .ThenInclude(u => u.Center)
            .Include(s => s.Plan)
            .AsQueryable();

        // CenterAdmin chỉ thấy subscription của student thuộc center mình
        if (query.CallerCenterId.HasValue)
        {
            subscriptionQuery = subscriptionQuery.Where(s => s.User.CenterId == query.CallerCenterId.Value);
        }

        if (query.UserId.HasValue)
        {
            subscriptionQuery = subscriptionQuery.Where(s => s.UserId == query.UserId.Value);
        }

        var list = await subscriptionQuery
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);

        var result = new List<AdminTransactionHistoryDto>();

        foreach (var sub in list)
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
                if (sub.StartDate < DateTime.UtcNow.AddHours(-24))
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
                status = "INACTIVE";
            }

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                !string.Equals(status, query.Status, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            result.Add(new AdminTransactionHistoryDto
            {
                Id = sub.Id,
                OrderCode = orderCode,
                UserId = sub.UserId,
                UserFullName = sub.User.FullName,
                Email = sub.User.Email,
                CenterName = sub.User.Center?.Name,
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

        return result;
    }
}
