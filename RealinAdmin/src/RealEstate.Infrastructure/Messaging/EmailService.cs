using Microsoft.Extensions.Logging;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Messaging;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendOtpAsync(string email, string otpCode)
    {
        // TODO: Implement actual email sending (e.g., via SES, SendGrid, etc.)
        _logger.LogInformation("OTP {OtpCode} sent to email {Email}", otpCode, email);
        await Task.CompletedTask;
    }
}
