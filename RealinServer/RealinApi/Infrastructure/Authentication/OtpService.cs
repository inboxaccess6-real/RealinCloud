using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;

namespace RealinApi.Infrastructure.Authentication;

public interface IOtpService
{
    Task<(bool Success, string? Code)> GenerateOtpAsync(string? email, string? phoneNumber, OtpDeliveryMethod method);
    Task<(bool Success, string? Error, Guid? UserId)> ValidateOtpAsync(string? email, string? phoneNumber, string code);
    Task<bool> CheckRateLimitAsync(string? email, string? phoneNumber);
}

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OtpService> _logger;
    private readonly int _otpExpirationMinutes;
    private readonly int _maxAttemptsPerWindow;
    private readonly int _rateLimitWindowMinutes;
    private readonly int _otpLength;

    public OtpService(AppDbContext context, IConfiguration configuration, ILogger<OtpService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        
        _otpExpirationMinutes = int.Parse(configuration["Otp:ExpirationMinutes"] ?? "10");
        _maxAttemptsPerWindow = int.Parse(configuration["Otp:MaxAttemptsPerWindow"] ?? "5");
        _rateLimitWindowMinutes = int.Parse(configuration["Otp:RateLimitWindowMinutes"] ?? "60");
        _otpLength = int.Parse(configuration["Otp:Length"] ?? "6");
    }

    public async Task<(bool Success, string? Code)> GenerateOtpAsync(
        string? email, 
        string? phoneNumber, 
        OtpDeliveryMethod method)
    {
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber))
        {
            return (false, null);
        }

        // Check rate limit
        if (!await CheckRateLimitAsync(email, phoneNumber))
        {
            _logger.LogWarning("Rate limit exceeded for {Email}/{PhoneNumber}", email, phoneNumber);
            return (false, null);
        }

        // Invalidate any existing OTP sessions for this email/phone
        var existingSessions = await _context.OtpSessions
            .Where(o => !o.IsVerified && 
                       ((!string.IsNullOrEmpty(email) && o.Email == email) ||
                        (!string.IsNullOrEmpty(phoneNumber) && o.PhoneNumber == phoneNumber)))
            .ToListAsync();

        if (existingSessions.Any())
        {
            _context.OtpSessions.RemoveRange(existingSessions);
        }

        // Generate OTP code
        var otpCode = GenerateRandomCode(_otpLength);

        // Create new OTP session
        var otpSession = new OtpSession
        {
            Id = Guid.NewGuid(),
            Email = email,
            PhoneNumber = phoneNumber,
            OtpCode = otpCode,
            DeliveryMethod = method,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_otpExpirationMinutes),
            IsVerified = false,
            AttemptCount = 0
        };

        _context.OtpSessions.Add(otpSession);
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Email}/{PhoneNumber}: {Code}", email, phoneNumber, otpCode);
        
        return (true, otpCode);
    }

    public async Task<(bool Success, string? Error, Guid? UserId)> ValidateOtpAsync(
        string? email, 
        string? phoneNumber, 
        string code)
    {
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber))
        {
            return (false, "Email or phone number is required", null);
        }

        // Find the OTP session
        var otpSession = await _context.OtpSessions
            .Where(o => !o.IsVerified &&
                       ((!string.IsNullOrEmpty(email) && o.Email == email) ||
                        (!string.IsNullOrEmpty(phoneNumber) && o.PhoneNumber == phoneNumber)))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otpSession == null)
        {
            return (false, "No OTP session found", null);
        }

        // Check expiration
        if (otpSession.ExpiresAt < DateTime.UtcNow)
        {
            return (false, "OTP has expired", null);
        }

        // Increment attempt count
        otpSession.AttemptCount++;

        // Check max attempts
        if (otpSession.AttemptCount > 3)
        {
            _context.OtpSessions.Remove(otpSession);
            await _context.SaveChangesAsync();
            return (false, "Too many invalid attempts", null);
        }

        // Validate code
        if (otpSession.OtpCode != code)
        {
            await _context.SaveChangesAsync();
            return (false, "Invalid OTP code", null);
        }

        // Mark as verified
        otpSession.IsVerified = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP validated successfully for {Email}/{PhoneNumber}", email, phoneNumber);

        return (true, null, otpSession.UserId);
    }

    public async Task<bool> CheckRateLimitAsync(string? email, string? phoneNumber)
    {
        var windowStart = DateTime.UtcNow.AddMinutes(-_rateLimitWindowMinutes);

        var attemptCount = await _context.OtpSessions
            .Where(o => o.CreatedAt >= windowStart &&
                       ((!string.IsNullOrEmpty(email) && o.Email == email) ||
                        (!string.IsNullOrEmpty(phoneNumber) && o.PhoneNumber == phoneNumber)))
            .CountAsync();

        return attemptCount < _maxAttemptsPerWindow;
    }

    private string GenerateRandomCode(int length)
    {
        var random = new Random();
        return string.Join("", Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
    }
}
