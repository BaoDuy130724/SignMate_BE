using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Commands.AssignLesson;

/// <summary>
/// Lệnh giao một bài học cho lớp kèm hạn nộp —
/// <c>POST /api/centers/{centerId}/classes/{classId}/lessons</c>.
/// </summary>
/// <param name="ClassId">Id lớp được giao bài.</param>
/// <param name="TeacherId">Id giáo viên giao bài (lấy từ JWT).</param>
/// <param name="Request">Bài học và hạn nộp.</param>
public record AssignLessonCommand(int ClassId, int TeacherId, AssignLessonRequest Request) : ICommand<Unit>;
