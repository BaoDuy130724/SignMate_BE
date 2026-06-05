using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 5;

    /// <summary>Khoảng chờ tối thiểu giữa 2 lần gửi OTP cho cùng một email (chống spam gửi lại).</summary>
    private const int ResendCooldownSeconds = 60;

    public OtpService(IMemoryCache cache, IEmailService emailService, ILogger<OtpService> logger)
    {
        _cache = cache;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateAndSendOtpAsync(string email, string purpose)
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

        try
        {
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var cacheKey = $"OTP_{purpose}_{email}";
            _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(OtpExpiryMinutes));

            if (purpose == "Register")
            {
                await _emailService.SendRegistrationOtpEmailAsync(email, otpCode);
            }
            else if (purpose == "ResetPassword")
            {
                await _emailService.SendPasswordResetEmailAsync(email, otpCode);
            }

            _logger.LogInformation("Generated OTP for {Email} ({Purpose})", email, purpose);
            return otpCode;
        }
        catch
        {
            // Gửi mail thất bại → gỡ cooldown để người dùng có thể thử lại ngay.
            _cache.Remove(cooldownKey);
            throw;
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
