using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Classes.Commands.RemoveStudent;

/// <summary>
/// Lệnh gỡ học viên ra khỏi lớp học của trung tâm — <c>DELETE /api/centers/{centerId}/classes/{classId}/students/{studentId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="ClassId">Id lớp học.</param>
/// <param name="StudentId">Id học viên cần gỡ.</param>
public record RemoveStudentCommand(int CenterId, int ClassId, int StudentId) : ICommand<Unit>;
