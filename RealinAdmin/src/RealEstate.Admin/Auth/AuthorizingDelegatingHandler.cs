using System.Net;
using System.Net.Http.Headers;
using RealEstate.Admin.Services;

namespace RealEstate.Admin.Auth;

public class AuthorizingDelegatingHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly AuthService _authService;

    public AuthorizingDelegatingHandler(TokenService tokenService, AuthService authService)
    {
        _tokenService = tokenService;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // If access token is expiring soon, try to refresh before sending the request
        if (await _tokenService.IsAccessTokenExpiringSoonAsync())
        {
            await _authService.TryRefreshTokenAsync();
        }

        var token = await _tokenService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // If 401, try refresh once and retry
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (await _authService.TryRefreshTokenAsync())
            {
                token = await _tokenService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    var retryRequest = await CloneRequestAsync(request);
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    response = await base.SendAsync(retryRequest, cancellationToken);
                }
            }
            else
            {
                await _tokenService.ClearTokensAsync();
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
