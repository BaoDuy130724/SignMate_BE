using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Queries.GetClassById;

/// <summary>
/// Truy vấn chi tiết một lớp học của trung tâm — <c>GET /api/centers/{centerId}/classes/{classId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="ClassId">Id lớp học.</param>
public record GetClassByIdQuery(int CenterId, int ClassId) : IQuery<ClassDto>;
