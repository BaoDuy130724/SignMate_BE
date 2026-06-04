using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Features.Lessons.Commands.CreateLesson;

/// <summary>
/// Lệnh tạo bài học con trong một khóa học (mặc định chưa publish) —
/// <c>POST /api/lessons/course/{courseId}</c>.
/// </summary>
/// <param name="CourseId">Id khóa học chứa bài học.</param>
/// <param name="Request">Thông tin bài học.</param>
public record CreateLessonCommand(int CourseId, CreateLessonRequest Request) : ICommand<LessonDto>;
