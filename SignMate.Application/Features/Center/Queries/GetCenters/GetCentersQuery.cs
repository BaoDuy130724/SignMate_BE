using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Center;

namespace SignMate.Application.Features.Center.Queries.GetCenters;

/// <summary>
/// Truy vấn danh sách tất cả trung tâm (dành cho SuperAdmin) — <c>GET /api/centers</c>.
/// </summary>
public record GetCentersQuery : IQuery<List<CenterDto>>;
