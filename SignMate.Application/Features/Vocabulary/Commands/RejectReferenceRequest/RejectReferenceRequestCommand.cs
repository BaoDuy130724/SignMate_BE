using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Vocabulary.Commands.RejectReferenceRequest;

/// <summary>
/// Lệnh từ chối một yêu cầu bơm video kèm lý do — <c>POST /api/vocabulary/requests/{requestId}/reject</c>.
/// </summary>
/// <param name="RequestId">Id yêu cầu cần từ chối.</param>
/// <param name="ReviewerId">Id quản trị viên từ chối (lấy từ JWT).</param>
/// <param name="Reason">Lý do từ chối hiển thị cho giáo viên.</param>
public record RejectReferenceRequestCommand(int RequestId, int ReviewerId, string Reason) : ICommand<Unit>;
