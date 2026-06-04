using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Features.Auth.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.Refresh;

/// <summary>
/// Handler cho <see cref="RefreshCommand"/>: áp dụng cơ chế refresh-token rotation — thu hồi token
/// hiện tại và phát hành token mới. Việc thu hồi token cũ và thêm token mới nằm trong cùng một
/// <c>SaveChangesAsync</c> nên đảm bảo Atomicity (không bao giờ tồn tại đồng thời 2 token hợp lệ).
/// </summary>
public class RefreshCommandHandler : IRequestHandler<RefreshCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var stored = await _unitOfWork.Repository<RefreshToken>().Query()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == command.RefreshToken && !r.IsRevoked, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");

        // Thu hồi token cũ và cấp token mới trong cùng một lần lưu (atomic rotation).
        stored.IsRevoked = true;
        var tokens = await TokenIssuer.IssueAsync(_unitOfWork, _tokenService, stored.User);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tokens;
    }
}
