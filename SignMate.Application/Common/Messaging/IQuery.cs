using MediatR;

namespace SignMate.Application.Common.Messaging;

/// <summary>
/// Marker cho một thao tác ĐỌC (read-side) trong CQRS.
/// Handler tương ứng chỉ truy vấn dữ liệu (thường projection EF Core + AsNoTracking),
/// không được thay đổi trạng thái hệ thống.
/// </summary>
/// <typeparam name="TResponse">Kiểu dữ liệu trả về cho client.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
