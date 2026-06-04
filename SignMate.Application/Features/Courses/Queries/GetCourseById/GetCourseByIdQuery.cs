using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Courses.Queries.GetCourseById;

/// <summary>
/// Truy vấn chi tiết một khóa học kèm danh sách bài học con — <c>GET /api/courses/{id}</c>.
/// </summary>
/// <param name="Id">Id khóa học.</param>
public record GetCourseByIdQuery(int Id) : IQuery<CourseDetailDto>;
