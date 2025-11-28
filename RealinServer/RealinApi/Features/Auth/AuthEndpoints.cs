using RealinApi.Features.Auth.Models;

namespace RealinApi.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/google", GoogleLogin)
            .WithName("GoogleLogin")
            .WithSummary("Authenticate with Google")
            .Produces<AuthResponse>(200)
            .Produces(400);

        group.MapPost("/apple", AppleLogin)
            .WithName("AppleLogin")
            .WithSummary("Authenticate with Apple")
            .Produces<AuthResponse>(200)
            .Produces(400);

        group.MapPost("/otp/request", RequestOtp)
            .WithName("RequestOtp")
            .WithSummary("Request OTP via SMS or Email")
            .Produces(200)
            .Produces(400)
            .Produces(429); // Rate limit

        group.MapPost("/otp/verify", VerifyOtp)
            .WithName("VerifyOtp")
            .WithSummary("Verify OTP and authenticate")
            .Produces<AuthResponse>(200)
            .Produces(400);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<AuthResponse>(200)
            .Produces(400);
    }

    private static async Task<IResult> GoogleLogin(
        GoogleLoginRequest request,
        IAuthService authService)
    {
        var (success, response, error) = await authService.GoogleLoginAsync(request);
        
        if (!success)
        {
            return Results.BadRequest(new { error });
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> AppleLogin(
        AppleLoginRequest request,
        IAuthService authService)
    {
        var (success, response, error) = await authService.AppleLoginAsync(request);
        
        if (!success)
        {
            return Results.BadRequest(new { error });
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> RequestOtp(
        OtpRequestRequest request,
        IAuthService authService)
    {
        var (success, message, error) = await authService.RequestOtpAsync(request);
        
        if (!success)
        {
            return error?.Contains("Rate limit") == true 
                ? Results.StatusCode(429) 
                : Results.BadRequest(new { error });
        }

        return Results.Ok(new { message });
    }

    private static async Task<IResult> VerifyOtp(
        OtpVerifyRequest request,
        IAuthService authService)
    {
        var (success, response, error) = await authService.VerifyOtpAsync(request);
        
        if (!success)
        {
            return Results.BadRequest(new { error });
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        IAuthService authService)
    {
        var (success, response, error) = await authService.RefreshTokenAsync(request);
        
        if (!success)
        {
            return Results.BadRequest(new { error });
        }

        return Results.Ok(response);
    }
}
