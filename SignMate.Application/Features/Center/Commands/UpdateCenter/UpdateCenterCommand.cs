using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Center;

namespace SignMate.Application.Features.Center.Commands.UpdateCenter;

/// <summary>
/// Lệnh cập nhật thông tin trung tâm — <c>PUT /api/centers/{id}</c>.
/// </summary>
/// <param name="Id">Trung tâm cần sửa.</param>
/// <param name="Request">Thông tin mới của trung tâm.</param>
public record UpdateCenterCommand(int Id, CenterDto Request) : ICommand<CenterDto>;
