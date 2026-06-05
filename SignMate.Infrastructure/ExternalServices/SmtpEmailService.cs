using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

/// <summary>
/// Triển khai <see cref="IEmailService"/> gửi email thật qua SMTP (MailKit).
/// Dùng cho OTP đăng ký và mã đặt lại mật khẩu. Cấu hình lấy từ <see cref="EmailSettings"/>.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task SendRegistrationOtpEmailAsync(string toEmail, string otpCode) =>
        SendAsync(
            toEmail,
            "Mã xác thực đăng ký SignMate",
            BuildOtpHtml(
                heading: "Xác thực đăng ký",
                intro: "Cảm ơn bạn đã đăng ký SignMate. Nhập mã dưới đây để hoàn tất tạo tài khoản:",
                code: otpCode));

    /// <inheritdoc />
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken) =>
        SendAsync(
            toEmail,
            "Mã đặt lại mật khẩu SignMate",
            BuildOtpHtml(
                heading: "Đặt lại mật khẩu",
                intro: "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Dùng mã dưới đây để tiếp tục:",
                code: resetToken));

    /// <summary>
    /// Soạn <see cref="MimeMessage"/> và gửi qua SMTP với STARTTLS/SSL theo cấu hình.
    /// Kết nối được tạo &amp; đóng cho mỗi lần gửi — phù hợp tần suất OTP thấp, tránh giữ socket treo.
    /// </summary>
    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var socketOptions = _settings.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect;

            await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation("Đã gửi email '{Subject}' tới {Email}.", subject, toEmail);
        }
        catch (Exception ex)
        {
            // Ném lại để middleware trả lỗi chuẩn ApiResponse — tránh "nuốt" lỗi gửi mail âm thầm.
            _logger.LogError(ex, "Gửi email '{Subject}' tới {Email} thất bại.", subject, toEmail);
            throw;
        }
    }

    /// <summary>Dựng nội dung HTML đơn giản, nổi bật mã OTP/token.</summary>
    private static string BuildOtpHtml(string heading, string intro, string code) => $$"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;padding:24px;color:#1f2937">
          <h2 style="color:#4f46e5;margin:0 0 12px">{{heading}}</h2>
          <p style="margin:0 0 16px;font-size:15px;line-height:1.5">{{intro}}</p>
          <div style="font-size:32px;font-weight:700;letter-spacing:8px;text-align:center;
                      background:#eef2ff;color:#4f46e5;border-radius:8px;padding:16px 0;margin:8px 0 16px">
            {{code}}
          </div>
          <p style="margin:0;font-size:13px;color:#6b7280">Mã có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
          <hr style="border:none;border-top:1px solid #e5e7eb;margin:20px 0" />
          <p style="margin:0;font-size:12px;color:#9ca3af">SignMate — Nền tảng học Ngôn ngữ Ký hiệu Việt Nam</p>
        </div>
        """;
}
