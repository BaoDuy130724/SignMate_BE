using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Vocabulary.Commands.ApproveReferenceRequest;

/// <summary>
/// Lệnh duyệt một yêu cầu bơm video: ghi keypoint do AI tách được vào từ vựng đích —
/// <c>POST /api/vocabulary/requests/{requestId}/approve</c>.
/// </summary>
/// <param name="RequestId">Id yêu cầu cần duyệt.</param>
/// <param name="ReviewerId">Id quản trị viên duyệt (lấy từ JWT).</param>
public record ApproveReferenceRequestCommand(int RequestId, int ReviewerId) : ICommand<Unit>;
