namespace RealEstate.Application.Interfaces;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string otpCode);
}
