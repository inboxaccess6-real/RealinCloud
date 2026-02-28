using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Auth;
using RealEstate.Application.Features.Auth.Commands;

namespace RealEstate.Api.Endpoints;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/google", async (GoogleLoginRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new GoogleLoginCommand(request.IdToken, request.DeviceInfo));
            return Results.Ok(new ApiResponse<AuthResponse>(true, result));
        });

        group.MapPost("/otp/request", async (OtpRequestRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new RequestOtpCommand(request.Method, request.Recipient));
            return Results.Ok(new ApiResponse<string>(true, result));
        });

        group.MapPost("/otp/verify", async (OtpVerifyRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new VerifyOtpCommand(request.Method, request.Recipient, request.Code, request.DeviceInfo));
            return Results.Ok(new ApiResponse<AuthResponse>(true, result));
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken));
            return Results.Ok(new ApiResponse<AuthResponse>(true, result));
        });

        return app;
    }
}
