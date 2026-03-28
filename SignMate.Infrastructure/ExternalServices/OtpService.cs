using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 5;

    public OtpService(IMemoryCache cache, IEmailService emailService, ILogger<OtpService> logger)
    {
        _cache = cache;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<string> GenerateAndSendOtpAsync(string email, string purpose)
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

        _logger.LogInformation("Generated OTP {Otp} for {Email} ({Purpose})", otpCode, email, purpose);
        return otpCode;
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
