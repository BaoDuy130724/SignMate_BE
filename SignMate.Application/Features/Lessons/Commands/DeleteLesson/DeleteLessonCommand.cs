using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Lessons.Commands.DeleteLesson;

/// <summary>
/// Lệnh xóa một bài học con — <c>DELETE /api/lessons/{id}</c>.
/// </summary>
/// <param name="LessonId">Id bài học cần xóa.</param>
public record DeleteLessonCommand(int LessonId) : ICommand<Unit>;
