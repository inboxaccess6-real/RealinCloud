using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IOtpService
{
    Task<(bool Success, string? Code)> GenerateOtpAsync(string recipient, OtpDeliveryMethod method);
    Task<(bool Success, string? Error, Guid? UserId)> ValidateOtpAsync(string recipient, OtpDeliveryMethod method, string code);
}
