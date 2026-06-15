using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 5;

    /// <summary>Khoảng chờ tối thiểu giữa 2 lần gửi OTP cho cùng một email (chống spam gửi lại).</summary>
    private const int ResendCooldownSeconds = 60;

    public OtpService(IMemoryCache cache, IServiceScopeFactory scopeFactory, ILogger<OtpService> logger)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task<string> GenerateAndSendOtpAsync(string email, string purpose)
    {
        var cooldownKey = $"OTP_CD_{purpose}_{email}";

        // Chặn gửi lại quá sớm cho cùng email (kể cả khi người dùng bấm liên tục).
        if (_cache.TryGetValue(cooldownKey, out DateTimeOffset cooldownUntil))
        {
            var remaining = (int)Math.Ceiling((cooldownUntil - DateTimeOffset.UtcNow).TotalSeconds);
            throw new TooManyRequestsException(
                $"Bạn vừa yêu cầu mã OTP. Vui lòng đợi {(remaining < 1 ? 1 : remaining)} giây rồi thử lại.");
        }

        // Đặt cooldown NGAY (trước khi gửi mail) để chặn double-click trong lúc đang gửi.
        var until = DateTimeOffset.UtcNow.AddSeconds(ResendCooldownSeconds);
        _cache.Set(cooldownKey, until, TimeSpan.FromSeconds(ResendCooldownSeconds));

        var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var cacheKey = $"OTP_{purpose}_{email}";
        _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(OtpExpiryMinutes));

        // Gửi email KHÔNG chặn response. SMTP (connect/auth/send) có thể mất >10s; nếu await
        // thì client (timeout ~15s) báo "lỗi mạng" giả dù OTP đã tạo và mail vẫn gửi. OTP đã
        // nằm trong cache nên hợp lệ ngay → trả về liền để client sang màn nhập OTP; mail tới sau.
        _ = SendOtpEmailAsync(email, purpose, otpCode, cooldownKey);

        _logger.LogInformation("Generated OTP for {Email} ({Purpose})", email, purpose);
        return Task.FromResult(otpCode);
    }

    /// <summary>
    /// Gửi email OTP ở nền. Tạo <b>scope DI mới</b> vì <see cref="IEmailService"/> là scoped —
    /// không được tái dùng scope của request (đã đóng khi handler trả về). Nuốt lỗi để task nền
    /// không "unobserved"; gửi hỏng thì gỡ cooldown cho phép gửi lại ngay.
    /// </summary>
    private async Task SendOtpEmailAsync(string email, string purpose, string otpCode, string cooldownKey)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            if (purpose == "Register")
                await emailService.SendRegistrationOtpEmailAsync(email, otpCode);
            else if (purpose == "ResetPassword")
                await emailService.SendPasswordResetEmailAsync(email, otpCode);
        }
        catch (Exception ex)
        {
            _cache.Remove(cooldownKey);
            _logger.LogError(ex, "Gửi OTP email tới {Email} ({Purpose}) thất bại.", email, purpose);
        }
    }

    public bool ValidateOtp(string email, string purpose, string otpCode)
    {
        var cacheKey = $"OTP_{purpose}_{email}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedOtp))
        {
            if (cachedOtp == otpCode)
            {
                // Remove OTP after successful validation (one-time use)
                _cache.Remove(cacheKey);
                return true;
            }
        }
        
        return false;
    }
}
