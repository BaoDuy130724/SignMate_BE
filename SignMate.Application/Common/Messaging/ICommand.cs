using MediatR;

namespace SignMate.Application.Common.Messaging;

/// <summary>
/// Marker cho một thao tác GHI (write-side) trong CQRS.
/// Tách biệt rõ ràng với <see cref="IQuery{TResponse}"/> để người đọc biết handler
/// này thay đổi trạng thái hệ thống (tạo/sửa/xóa) và thường đi kèm Unit of Work + transaction.
/// </summary>
/// <typeparam name="TResponse">Kiểu kết quả trả về sau khi thực thi lệnh.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker cho một thao tác GHI không cần trả về dữ liệu (trả <see cref="Unit"/>).
/// </summary>
public interface ICommand : IRequest<Unit>
{
}
