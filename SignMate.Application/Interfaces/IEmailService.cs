namespace SignMate.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    Task SendRegistrationOtpEmailAsync(string toEmail, string otpCode);
}
