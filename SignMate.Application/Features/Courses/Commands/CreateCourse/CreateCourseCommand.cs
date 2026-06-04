using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Courses.Commands.CreateCourse;

/// <summary>
/// Lệnh tạo khóa học mới (mặc định chưa publish) — <c>POST /api/courses</c>.
/// </summary>
/// <param name="Request">Thông tin khóa học.</param>
/// <param name="CreatedBy">Id người tạo lấy từ JWT.</param>
public record CreateCourseCommand(CreateCourseRequest Request, int CreatedBy) : ICommand<CourseDto>;
