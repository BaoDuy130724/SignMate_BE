using Microsoft.Extensions.Logging;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        // In a real app, integrate SendGrid / SMTP here.
        _logger.LogInformation("--- MOCK EMAIL SENT ---");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Subject: Password Reset Request");
        _logger.LogInformation("Body: Your password reset token/code is: {Token}", resetToken);
        _logger.LogInformation("-----------------------");
        
        return Task.CompletedTask;
    }

    public Task SendRegistrationOtpEmailAsync(string toEmail, string otpCode)
    {
        _logger.LogInformation("--- MOCK REGISTRATION OTP SENT ---");
        _logger.LogInformation("To: {Email}", toEmail);
        _logger.LogInformation("Subject: Complete your SignMate registration");
        _logger.LogInformation("Body: Your OTP code is: {Token}. It expires in 5 minutes.", otpCode);
        _logger.LogInformation("-----------------------");
        
        return Task.CompletedTask;
    }
}
