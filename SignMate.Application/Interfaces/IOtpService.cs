namespace SignMate.Application.Interfaces;

public interface IOtpService
{
    /// <summary>
    /// Generates a 6-digit OTP for the given email, stores it in cache, and sends it via email.
    /// </summary>
    Task<string> GenerateAndSendOtpAsync(string email, string purpose);

    /// <summary>
    /// Validates the given OTP against the cached value for that email and purpose.
    /// Returns true if valid, false otherwise.
    /// </summary>
    bool ValidateOtp(string email, string purpose, string otpCode);
}
