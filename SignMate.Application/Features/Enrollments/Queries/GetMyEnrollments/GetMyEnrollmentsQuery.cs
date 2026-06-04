using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Enrollment;

namespace SignMate.Application.Features.Enrollments.Queries.GetMyEnrollments;

/// <summary>
/// Truy vấn danh sách khóa học đã đăng ký của học viên kèm tiến độ hoàn thành —
/// <c>GET /api/enrollments/me</c>.
/// </summary>
/// <param name="UserId">Id học viên lấy từ JWT.</param>
public record GetMyEnrollmentsQuery(int UserId) : IQuery<List<EnrollmentDto>>;
