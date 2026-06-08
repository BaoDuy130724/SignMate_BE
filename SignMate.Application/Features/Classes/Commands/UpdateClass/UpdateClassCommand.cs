using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Commands.UpdateClass;

/// <summary>
/// Lệnh sửa thông tin lớp học (Name, TeacherId) của trung tâm — <c>PUT /api/centers/{centerId}/classes/{classId}</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="ClassId">Id lớp học cần cập nhật.</param>
/// <param name="Request">Thông tin cập nhật mới.</param>
public record UpdateClassCommand(int CenterId, int ClassId, UpdateClassRequest Request) : ICommand<ClassDto>;
