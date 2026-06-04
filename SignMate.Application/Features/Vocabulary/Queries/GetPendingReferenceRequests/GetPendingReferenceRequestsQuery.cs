using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Vocabulary;

namespace SignMate.Application.Features.Vocabulary.Queries.GetPendingReferenceRequests;

/// <summary>
/// Truy vấn danh sách yêu cầu bơm video đã sẵn sàng để quản trị viên duyệt —
/// <c>GET /api/vocabulary/pending-references</c>.
/// </summary>
public record GetPendingReferenceRequestsQuery : IQuery<List<PendingReferenceRequestDto>>;
