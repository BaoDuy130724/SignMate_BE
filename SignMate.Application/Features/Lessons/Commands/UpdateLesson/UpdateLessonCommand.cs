using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Lessons.Commands.UpdateLesson;

/// <summary>
/// Lệnh cập nhật một phần thông tin bài học con — <c>PUT /api/lessons/{id}</c>.
/// </summary>
/// <param name="LessonId">Id bài học cần cập nhật.</param>
/// <param name="Request">Các trường cần cập nhật (chỉ áp dụng trường khác null).</param>
public record UpdateLessonCommand(int LessonId, UpdateLessonRequest Request) : ICommand<LessonDto>;
