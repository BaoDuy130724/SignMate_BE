using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.ExternalServices;

/// <summary>
/// Hiện thực <see cref="ITokenService"/>: ký JWT bằng khóa đối xứng và sinh refresh token ngẫu nhiên.
/// Toàn bộ tham số nhạy cảm (Secret) và chính sách thời hạn được đọc từ cấu hình ứng dụng.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) CreateAccessToken(User user)
    {
        var expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("centerId", user.CenterId?.ToString() ?? "")
        };

        var jwt = new JwtSecurityToken(claims: claims, expires: expiresAt, signingCredentials: credentials);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return (token, expiresAt);
    }

    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) CreateRefreshToken()
    {
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30");
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (token, DateTime.UtcNow.AddDays(refreshDays));
    }
}
