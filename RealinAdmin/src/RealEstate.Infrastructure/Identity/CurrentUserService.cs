using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var claim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? httpContext?.User?.FindFirst("sub")
                ?? httpContext?.User?.FindFirst("userId");

            return claim is not null && Guid.TryParse(claim.Value, out var userId)
                ? userId
                : null;
        }
    }

    public string? Role
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
