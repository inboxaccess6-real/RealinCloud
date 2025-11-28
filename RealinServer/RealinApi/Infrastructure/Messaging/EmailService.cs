namespace RealinApi.Infrastructure.Messaging;

public interface IEmailService
{
    Task<bool> SendOtpAsync(string email, string otpCode);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendOtpAsync(string email, string otpCode)
    {
        // TODO: Integrate with email provider (SendGrid, AWS SES, etc.)
        // For now, just log the OTP
        _logger.LogInformation("Email OTP to {Email}: {OtpCode}", email, otpCode);
        
        // Simulate async operation
        await Task.Delay(100);
        
        return true;
    }
}
