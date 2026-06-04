using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Center;

namespace SignMate.Application.Features.Center.Commands.CreateCenter;

/// <summary>
/// Lệnh tạo trung tâm mới (mặc định đang hoạt động) — <c>POST /api/centers</c>.
/// </summary>
/// <param name="Request">Thông tin trung tâm.</param>
public record CreateCenterCommand(CenterDto Request) : ICommand<CenterDto>;
