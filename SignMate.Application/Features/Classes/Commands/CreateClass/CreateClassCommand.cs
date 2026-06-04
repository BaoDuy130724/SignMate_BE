using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Commands.CreateClass;

/// <summary>
/// Lệnh tạo lớp học mới trong trung tâm — <c>POST /api/centers/{centerId}/classes</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm chứa lớp.</param>
/// <param name="Request">Tên lớp và giáo viên phụ trách.</param>
public record CreateClassCommand(int CenterId, CreateClassRequest Request) : ICommand<ClassDto>;
