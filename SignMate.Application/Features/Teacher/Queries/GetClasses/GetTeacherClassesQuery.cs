using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Teacher.Queries.GetClasses;

/// <summary>
/// Truy vấn danh sách lớp học giáo viên phụ trách kèm học sinh và độ chính xác trung bình —
/// <c>GET /api/teacher/classes</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên lấy từ JWT.</param>
public record GetTeacherClassesQuery(int TeacherId) : IQuery<List<ClassDto>>;
