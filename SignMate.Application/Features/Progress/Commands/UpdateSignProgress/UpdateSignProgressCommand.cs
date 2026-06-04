using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Progress;

namespace SignMate.Application.Features.Progress.Commands.UpdateSignProgress;

/// <summary>
/// Lệnh cập nhật tiến độ luyện một ký hiệu (đã thành thạo chưa, số lần thử) — <c>PUT /api/progress/sign</c>.
/// </summary>
/// <param name="UserId">Id người học lấy từ JWT.</param>
/// <param name="Request">Ký hiệu và trạng thái thành thạo mới.</param>
public record UpdateSignProgressCommand(int UserId, UpdateSignProgressRequest Request) : ICommand<Unit>;
