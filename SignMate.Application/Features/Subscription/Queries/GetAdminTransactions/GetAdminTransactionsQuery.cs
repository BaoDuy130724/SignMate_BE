using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetAdminTransactions;

/// <summary>
/// Truy vấn lịch sử giao dịch toàn hệ thống hoặc theo trung tâm — <c>GET /api/subscription/admin/transactions</c>.
/// </summary>
/// <param name="CallerCenterId">Id trung tâm của caller; null = SuperAdmin (toàn bộ hệ thống).</param>
/// <param name="UserId">Lọc theo Id người dùng (tùy chọn).</param>
/// <param name="FromDate">Thời gian bắt đầu lọc (tùy chọn).</param>
/// <param name="ToDate">Thời gian kết thúc lọc (tùy chọn).</param>
public record GetAdminTransactionsQuery(
    int? CallerCenterId = null,
    int? UserId = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IQuery<List<AdminTransactionHistoryDto>>;
