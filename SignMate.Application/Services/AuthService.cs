using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly IOtpService _otpService;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _otpService = otpService;
    }

    public async Task SendRegisterOtpAsync(SendOtpRequest request)
    {
        if (await _unitOfWork.Repository<User>().Query().AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already exists.");

        await _otpService.GenerateAndSendOtpAsync(request.Email, "Register");
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _unitOfWork.Repository<User>().Query().AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already exists.");

        if (!_otpService.ValidateOtp(request.Email, "Register", request.OtpCode))
            throw new UnauthorizedAccessException("Invalid or expired OTP code.");

        var user = new User
        {
            Id = 0,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);

        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            Id = 0,
            UserId = user.Id,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _unitOfWork.SaveChangesAsync();
        return await GenerateTokens(user);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Email == request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await GenerateTokens(user);
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken)
    {
        var stored = await _unitOfWork.Repository<RefreshToken>().Query()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        stored.IsRevoked = true;

        var tokens = await GenerateTokens(stored.User);
        await _unitOfWork.SaveChangesAsync();
        return tokens;
    }

    public async Task LogoutAsync(int userId, string refreshToken)
    {
        var stored = await _unitOfWork.Repository<RefreshToken>().Query()
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId && !r.IsRevoked);

        if (stored != null)
        {
            stored.IsRevoked = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return; // Silent return for security

        await _otpService.GenerateAndSendOtpAsync(request.Email, "ResetPassword");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null)
            throw new UnauthorizedAccessException("Invalid request.");

        if (!_otpService.ValidateOtp(request.Email, "ResetPassword", request.Token))
            throw new UnauthorizedAccessException("Invalid or expired OTP code.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId) 
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid current password.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<TokenResponse> GenerateTokens(User user)
    {
        var expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(claims: claims, expires: expiresAt, signingCredentials: creds);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenStr = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            Id = 0,
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false
        });
        await _unitOfWork.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
            ExpiresAt = expiresAt
        };
    }
}
