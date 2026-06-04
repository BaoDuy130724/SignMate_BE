using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Courses.Queries.GetCourses;

/// <summary>
/// Truy vấn danh sách khóa học có lọc theo từ khóa và cấp độ — <c>GET /api/courses</c>.
/// </summary>
/// <param name="Search">Từ khóa tìm trong tiêu đề/mô tả (tùy chọn).</param>
/// <param name="Level">Cấp độ lọc (Beginner/Intermediate/Advanced), tùy chọn.</param>
/// <param name="IncludeUnpublished">True để lấy cả khóa chưa publish (dùng cho quản trị).</param>
public record GetCoursesQuery(string? Search, string? Level, bool IncludeUnpublished)
    : IQuery<List<CourseDto>>;
