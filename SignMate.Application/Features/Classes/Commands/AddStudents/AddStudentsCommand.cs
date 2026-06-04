using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Features.Classes.Commands.AddStudents;

/// <summary>
/// Lệnh thêm học viên vào một lớp — <c>POST /api/centers/{centerId}/classes/{classId}/students</c>.
/// </summary>
/// <param name="ClassId">Id lớp học.</param>
/// <param name="Request">Danh sách Id học viên cần thêm.</param>
public record AddStudentsCommand(int ClassId, AddStudentsRequest Request) : ICommand<Unit>;
