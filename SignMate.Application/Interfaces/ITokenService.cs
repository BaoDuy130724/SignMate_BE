using SignMate.Domain.Entities;

namespace SignMate.Application.Interfaces;

/// <summary>
/// Trừu tượng hóa việc phát hành token xác thực. Đặt ở tầng Application nhưng được hiện thực
/// ở Infrastructure (đọc khóa bí mật/thời hạn từ cấu hình, ký JWT) — tuân thủ Dependency Inversion:
/// handler nghiệp vụ không phụ thuộc chi tiết JWT/cấu hình.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Tạo access token (JWT) chứa claim định danh và vai trò của người dùng.
    /// </summary>
    /// <returns>Chuỗi token và thời điểm hết hạn.</returns>
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);

    /// <summary>
    /// Sinh refresh token ngẫu nhiên kèm thời hạn cấu hình sẵn (chưa lưu DB).
    /// </summary>
    (string Token, DateTime ExpiresAt) CreateRefreshToken();
}
