using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Subscription;

namespace SignMate.Application.Features.Subscription.Queries.GetMyTransactions;

/// <summary>
/// Truy vấn lịch sử giao dịch/đăng ký gói của người dùng hiện tại — <c>GET /api/subscription/my-history</c>.
/// Tự động đồng bộ trạng thái thực tế từ PayOS đối với các giao dịch đang chờ (Pending).
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
public record GetMyTransactionsQuery(int UserId) : IQuery<List<TransactionHistoryDto>>;
