using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Center.Commands.DeleteCenter;

/// <summary>
/// Lệnh xóa trung tâm — <c>DELETE /api/centers/{id}</c>.
/// </summary>
/// <param name="Id">Trung tâm cần xóa.</param>
public record DeleteCenterCommand(int Id) : ICommand;
