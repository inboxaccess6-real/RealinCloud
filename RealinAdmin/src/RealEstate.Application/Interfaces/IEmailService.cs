namespace RealEstate.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string email, string otpCode);
}
