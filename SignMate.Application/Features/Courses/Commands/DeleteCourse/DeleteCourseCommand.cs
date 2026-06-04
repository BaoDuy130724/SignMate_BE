using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Courses.Commands.DeleteCourse;

/// <summary>
/// Lệnh xóa một khóa học — <c>DELETE /api/courses/{id}</c>.
/// </summary>
/// <param name="Id">Id khóa học cần xóa.</param>
public record DeleteCourseCommand(int Id) : ICommand<Unit>;
