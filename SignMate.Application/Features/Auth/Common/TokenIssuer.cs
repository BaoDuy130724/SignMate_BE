using SignMate.Application.DTOs.Auth;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Common;

/// <summary>
/// Helper phát hành cặp token cho một người dùng: tạo access token (JWT), sinh refresh token,
/// lưu refresh token vào kho và trả về <see cref="TokenResponse"/>.
/// Tập trung logic này một chỗ để Login/Register/Refresh dùng chung (DRY), tránh lệch chính sách token.
/// </summary>
internal static class TokenIssuer
{
    /// <summary>
    /// Phát hành cặp access/refresh token. Refresh token được thêm vào Unit of Work nhưng
    /// <b>không</b> tự gọi SaveChanges — caller chịu trách nhiệm lưu (và bao transaction nếu cần)
    /// để đảm bảo tính atomic khi đi kèm các thao tác ghi khác.
    /// </summary>
    public static async Task<TokenResponse> IssueAsync(
        IUnitOfWork unitOfWork, ITokenService tokenService, User user)
    {
        var access = tokenService.CreateAccessToken(user);
        var refresh = tokenService.CreateRefreshToken();

        await unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            User = user,
            Token = refresh.Token,
            ExpiresAt = refresh.ExpiresAt,
            IsRevoked = false
        });

        return new TokenResponse
        {
            AccessToken = access.Token,
            RefreshToken = refresh.Token,
            ExpiresAt = access.ExpiresAt
        };
    }
}
