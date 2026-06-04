using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Enrollment;

namespace SignMate.Application.Features.Enrollments.Commands.Enroll;

/// <summary>
/// Lệnh đăng ký học viên vào một khóa học — <c>POST /api/enrollments</c>.
/// </summary>
/// <param name="UserId">Id học viên lấy từ JWT.</param>
/// <param name="Request">Khóa học cần đăng ký.</param>
public record EnrollCommand(int UserId, EnrollRequest Request) : ICommand<EnrollmentResultDto>;
