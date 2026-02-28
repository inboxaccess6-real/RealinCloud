using Microsoft.Extensions.Logging;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Messaging;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public async Task SendOtpAsync(string phoneNumber, string otpCode)
    {
        // TODO: Implement actual SMS sending (e.g., via Twilio, AWS SNS, etc.)
        _logger.LogInformation("OTP {OtpCode} sent to phone {PhoneNumber}", otpCode, phoneNumber);
        await Task.CompletedTask;
    }
}
