namespace SignMate.Infrastructure.ExternalServices;

/// <summary>
/// Cấu hình máy chủ SMTP để gửi email thật (đọc từ section <c>"Email"</c> của appsettings/User Secrets).
/// Khi <see cref="Host"/> để trống, hệ thống tự rơi về <see cref="MockEmailService"/> (chỉ log) để
/// môi trường dev không bắt buộc phải cấu hình SMTP.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>Địa chỉ SMTP host, ví dụ <c>smtp.gmail.com</c>. Để trống = tắt gửi mail thật.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Cổng SMTP. Gmail/Outlook dùng 587 (STARTTLS).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Tài khoản đăng nhập SMTP (thường trùng địa chỉ gửi).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Mật khẩu/App Password của tài khoản SMTP. Nên đặt trong User Secrets, không commit.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Địa chỉ hiển thị ở trường "From". Mặc định lấy theo <see cref="Username"/> nếu bỏ trống.</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>Tên hiển thị người gửi.</summary>
    public string FromName { get; set; } = "SignMate";

    /// <summary>Dùng STARTTLS (cổng 587). Đặt false nếu dùng SSL trực tiếp cổng 465.</summary>
    public bool UseStartTls { get; set; } = true;
}
