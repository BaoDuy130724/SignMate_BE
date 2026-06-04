using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Teacher.Queries.GetStudents;

/// <summary>
/// Truy vấn danh sách toàn bộ học viên giáo viên đang quản lý — <c>GET /api/teacher/students</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên lấy từ JWT.</param>
public record GetTeacherStudentsQuery(int TeacherId) : IQuery<List<ClassStudentDto>>;
