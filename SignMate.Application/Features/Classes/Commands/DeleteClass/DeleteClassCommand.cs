using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Classes.Commands.DeleteClass;

/// <summary>
/// Lệnh xóa lớp học thuộc trung tâm — <c>DELETE /api/centers/{centerId}/classes/{classId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="ClassId">Id lớp học cần xóa.</param>
public record DeleteClassCommand(int CenterId, int ClassId) : ICommand<Unit>;
