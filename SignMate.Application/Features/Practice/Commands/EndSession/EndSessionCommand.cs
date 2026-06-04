using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Practice.Commands.EndSession;

/// <summary>
/// Lệnh kết thúc một phiên luyện tập — <c>POST /api/practice/session/end</c>.
/// </summary>
/// <param name="UserId">Id người luyện tập lấy từ JWT (đảm bảo chỉ đóng phiên của chính mình).</param>
/// <param name="SessionId">Id phiên cần kết thúc.</param>
public record EndSessionCommand(int UserId, int SessionId) : ICommand<Unit>;
