using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Features.Teacher.Queries.GetStudentComments;

/// <summary>
/// Truy vấn các nhận xét đã gửi cho một học viên — <c>GET /api/teacher/students/{studentId}/comments</c>.
/// </summary>
/// <param name="StudentId">Id học viên.</param>
public record GetStudentCommentsQuery(int StudentId) : IQuery<List<TeacherCommentDto>>;
