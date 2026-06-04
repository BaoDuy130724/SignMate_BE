using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Queries.GetClasses;

/// <summary>
/// Truy vấn danh sách lớp học của một trung tâm kèm học viên và độ chính xác —
/// <c>GET /api/centers/{centerId}/classes</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
public record GetClassesQuery(int CenterId) : IQuery<List<ClassDto>>;
