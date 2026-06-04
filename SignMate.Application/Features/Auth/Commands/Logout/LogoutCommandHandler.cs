using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler cho <see cref="LogoutCommand"/>: thu hồi refresh token nếu còn hiệu lực.
/// Idempotent — nếu token không tồn tại hoặc đã thu hồi thì coi như đã đăng xuất, không báo lỗi.
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var stored = await _unitOfWork.Repository<RefreshToken>().Query()
            .FirstOrDefaultAsync(
                r => r.Token == command.RefreshToken && r.UserId == command.UserId && !r.IsRevoked,
                cancellationToken);

        if (stored is not null)
        {
            stored.IsRevoked = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
