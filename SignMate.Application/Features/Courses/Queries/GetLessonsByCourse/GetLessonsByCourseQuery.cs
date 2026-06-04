using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Courses.Queries.GetLessonsByCourse;

/// <summary>
/// Truy vấn danh sách bài học con của một khóa học — <c>GET /api/courses/{id}/lessons</c>.
/// </summary>
/// <param name="CourseId">Id khóa học.</param>
public record GetLessonsByCourseQuery(int CourseId) : IQuery<List<LessonDto>>;
