using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Features.Auth.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler cho <see cref="LoginCommand"/>: xác thực thông tin đăng nhập (BCrypt) rồi phát hành token.
/// Trả thông điệp lỗi chung chung khi sai email/mật khẩu để tránh lộ email nào tồn tại.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Email == command.Request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");

        if (!BCrypt.Net.BCrypt.Verify(command.Request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");

        // Phát hành token + lưu refresh token (một thao tác ghi → một SaveChanges là atomic).
        var tokens = await TokenIssuer.IssueAsync(_unitOfWork, _tokenService, user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tokens;
    }
}
