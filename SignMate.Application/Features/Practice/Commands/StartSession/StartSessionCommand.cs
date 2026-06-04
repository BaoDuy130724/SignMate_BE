using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Practice;

namespace SignMate.Application.Features.Practice.Commands.StartSession;

/// <summary>
/// Lệnh khởi tạo một phiên luyện tập cho một ký hiệu — <c>POST /api/practice/session/start</c>.
/// </summary>
/// <param name="UserId">Id người luyện tập lấy từ JWT.</param>
/// <param name="SignId">Id ký hiệu được luyện trong phiên.</param>
public record StartSessionCommand(int UserId, int SignId) : ICommand<StartSessionResponse>;
