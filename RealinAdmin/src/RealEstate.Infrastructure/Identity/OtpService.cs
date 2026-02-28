using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Identity;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OtpService> _logger;
    private readonly int _expirationMinutes;
    private readonly int _maxAttemptsPerWindow;
    private readonly int _rateLimitWindowMinutes;
    private readonly int _otpLength;

    public OtpService(AppDbContext context, IConfiguration configuration, ILogger<OtpService> logger)
    {
        _context = context;
        _logger = logger;
        _expirationMinutes = int.TryParse(configuration["Otp:ExpirationMinutes"], out var exp) ? exp : 10;
        _maxAttemptsPerWindow = int.TryParse(configuration["Otp:MaxAttemptsPerWindow"], out var max) ? max : 5;
        _rateLimitWindowMinutes = int.TryParse(configuration["Otp:RateLimitWindowMinutes"], out var win) ? win : 60;
        _otpLength = int.TryParse(configuration["Otp:Length"], out var len) ? len : 6;
    }

    public async Task<(bool Success, string? Code)> GenerateOtpAsync(string recipient, OtpDeliveryMethod method)
    {
        if (!await CheckRateLimitAsync(recipient, method))
        {
            _logger.LogWarning("Rate limit exceeded for {Recipient} via {Method}", recipient, method);
            return (false, null);
        }

        // Invalidate existing active sessions
        var existingSessions = await _context.OtpSessions
            .Where(s => !s.IsVerified
                && s.ExpiresAt > DateTime.UtcNow
                && ((method == OtpDeliveryMethod.Email && s.Email == recipient)
                    || (method == OtpDeliveryMethod.Sms && s.PhoneNumber == recipient))
                && s.DeliveryMethod == method)
            .ToListAsync();

        foreach (var session in existingSessions)
        {
            session.ExpiresAt = DateTime.UtcNow;
        }

        var code = GenerateRandomCode(_otpLength);

        var otpSession = new OtpSession
        {
            Id = Guid.NewGuid(),
            Email = method == OtpDeliveryMethod.Email ? recipient : null,
            PhoneNumber = method == OtpDeliveryMethod.Sms ? recipient : null,
            OtpCode = code,
            DeliveryMethod = method,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            IsVerified = false,
            AttemptCount = 0
        };

        _context.OtpSessions.Add(otpSession);
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Recipient} via {Method}", recipient, method);

        return (true, code);
    }

    public async Task<(bool Success, string? Error, Guid? UserId)> ValidateOtpAsync(
        string recipient, OtpDeliveryMethod method, string code)
    {
        var session = await _context.OtpSessions
            .Where(s => !s.IsVerified
                && s.DeliveryMethod == method
                && ((method == OtpDeliveryMethod.Email && s.Email == recipient)
                    || (method == OtpDeliveryMethod.Sms && s.PhoneNumber == recipient)))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (session is null)
        {
            return (false, "No active OTP session found.", null);
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            return (false, "OTP has expired.", null);
        }

        session.AttemptCount++;

        if (session.AttemptCount > 3)
        {
            session.ExpiresAt = DateTime.UtcNow; // Invalidate
            await _context.SaveChangesAsync();
            return (false, "Maximum verification attempts exceeded.", null);
        }

        if (!string.Equals(session.OtpCode, code, StringComparison.Ordinal))
        {
            await _context.SaveChangesAsync();
            return (false, "Invalid OTP code.", null);
        }

        session.IsVerified = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP verified for {Recipient} via {Method}", recipient, method);

        return (true, null, session.UserId);
    }

    private async Task<bool> CheckRateLimitAsync(string recipient, OtpDeliveryMethod method)
    {
        var windowStart = DateTime.UtcNow.AddMinutes(-_rateLimitWindowMinutes);

        var recentCount = await _context.OtpSessions
            .CountAsync(s => s.CreatedAt >= windowStart
                && s.DeliveryMethod == method
                && ((method == OtpDeliveryMethod.Email && s.Email == recipient)
                    || (method == OtpDeliveryMethod.Sms && s.PhoneNumber == recipient)));

        return recentCount < _maxAttemptsPerWindow;
    }

    private static string GenerateRandomCode(int length)
    {
        var maxValue = (int)Math.Pow(10, length);
        var code = RandomNumberGenerator.GetInt32(0, maxValue);
        return code.ToString().PadLeft(length, '0');
    }
}
