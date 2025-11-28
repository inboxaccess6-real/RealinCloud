namespace RealinApi.Infrastructure.Messaging;

public interface ISmsService
{
    Task<bool> SendOtpAsync(string phoneNumber, string otpCode);
}

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        // TODO: Integrate with SMS provider (Twilio, AWS SNS, etc.)
        // For now, just log the OTP
        _logger.LogInformation("SMS OTP to {PhoneNumber}: {OtpCode}", phoneNumber, otpCode);
        
        // Simulate async operation
        await Task.Delay(100);
        
        return true;
    }
}
