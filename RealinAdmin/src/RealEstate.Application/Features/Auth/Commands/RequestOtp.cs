using MediatR;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Auth.Commands;

public record RequestOtpCommand(LoginMethod Method, string Recipient) : IRequest<string>;

public class RequestOtpHandler : IRequestHandler<RequestOtpCommand, string>
{
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;

    public RequestOtpHandler(
        IOtpService otpService,
        ISmsService smsService,
        IEmailService emailService)
    {
        _otpService = otpService;
        _smsService = smsService;
        _emailService = emailService;
    }

    public async Task<string> Handle(RequestOtpCommand command, CancellationToken cancellationToken)
    {
        var deliveryMethod = command.Method == LoginMethod.Email
            ? OtpDeliveryMethod.Email
            : OtpDeliveryMethod.Sms;

        var (success, code) = await _otpService.GenerateOtpAsync(command.Recipient, deliveryMethod);

        if (!success || code is null)
            throw new BusinessRuleException("Too many OTP requests. Please try again later.");

        if (command.Method == LoginMethod.Email)
            await _emailService.SendOtpAsync(command.Recipient, code);
        else
            await _smsService.SendOtpAsync(command.Recipient, code);

        return "OTP sent successfully.";
    }
}
