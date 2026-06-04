using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Courses.Commands.UpdateCourse;

/// <summary>
/// Lệnh cập nhật một phần thông tin khóa học (kể cả publish/unpublish) — <c>PUT /api/courses/{id}</c>.
/// </summary>
/// <param name="Id">Id khóa học cần cập nhật.</param>
/// <param name="Request">Các trường cần cập nhật (chỉ áp dụng trường khác null).</param>
public record UpdateCourseCommand(int Id, UpdateCourseRequest Request) : ICommand<CourseDto>;
