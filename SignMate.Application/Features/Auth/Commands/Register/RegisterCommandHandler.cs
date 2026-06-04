using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Features.Auth.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler cho <see cref="RegisterCommand"/>: xác thực OTP, tạo <see cref="User"/> kèm bản ghi
/// <see cref="Streak"/> khởi tạo, rồi phát hành token đăng nhập.
/// User, Streak và RefreshToken được tạo trong cùng một <c>SaveChangesAsync</c> — EF gói chúng
/// trong một transaction nên đảm bảo Atomicity (hoặc tạo trọn vẹn, hoặc không gì cả).
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, TokenResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var emailExists = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            throw new ConflictException("Email đã được sử dụng.");

        if (!_otpService.ValidateOtp(request.Email, "Register", request.OtpCode))
            throw new UnauthorizedAccessException("Mã OTP không đúng hoặc đã hết hạn.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<User>().AddAsync(user);

        // Khởi tạo streak rỗng cho học viên mới (dùng navigation để EF tự gán khóa ngoại).
        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            User = user,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        var tokens = await TokenIssuer.IssueAsync(_unitOfWork, _tokenService, user);

        // Một SaveChanges duy nhất ghi User + Streak + RefreshToken trong cùng transaction của EF.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tokens;
    }
}
