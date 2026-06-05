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
/// Lưu hai bước: (1) ghi User + Streak để DB cấp <c>Id</c> identity, (2) phát hành &amp; lưu refresh token.
/// Phải tách như vậy vì JWT nhúng <c>user.Id</c> — không thể phát hành token khi Id chưa được sinh.
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

        // Lưu trước để DB cấp giá trị Id (identity) cho user — BẮT BUỘC trước khi phát hành token,
        // vì JWT nhúng user.Id vào claim NameIdentifier. Phát hành token khi Id còn 0 sẽ tạo JWT lỗi.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Giờ user.Id đã có giá trị thật → token mang đúng định danh; lưu lần hai cho refresh token.
        var tokens = await TokenIssuer.IssueAsync(_unitOfWork, _tokenService, user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return tokens;
    }
}
