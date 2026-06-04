using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Queries.GetClassStudents;

/// <summary>
/// Truy vấn danh sách học viên trong một lớp — <c>GET /api/centers/{centerId}/classes/{classId}/students</c>.
/// </summary>
/// <param name="ClassId">Id lớp học.</param>
public record GetClassStudentsQuery(int ClassId) : IQuery<List<ClassStudentDto>>;
