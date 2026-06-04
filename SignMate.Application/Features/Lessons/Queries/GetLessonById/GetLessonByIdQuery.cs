using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Lessons.Queries.GetLessonById;

/// <summary>
/// Truy vấn chi tiết một bài học con kèm danh sách từ vựng — <c>GET /api/lessons/{id}</c>.
/// </summary>
/// <param name="LessonId">Id bài học.</param>
public record GetLessonByIdQuery(int LessonId) : IQuery<LessonDetailDto>;
