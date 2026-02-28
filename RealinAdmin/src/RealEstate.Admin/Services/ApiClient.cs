using System.Net.Http.Json;
using RealEstate.Application.Common;

namespace RealEstate.Admin.Services;

public record ApiResult<T>(bool Success, T? Data, string? Error, int StatusCode);

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl;

    public ApiClient(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _apiBaseUrl = (config.GetValue<string>("ApiBaseUrl") ?? "").TrimEnd('/');
    }

    private HttpClient Client => _httpClientFactory.CreateClient("AuthenticatedApi");

    /// <summary>
    /// Resolves a relative URL (e.g. /uploads/media/...) to an absolute URL using the API base.
    /// Already-absolute URLs (http/https) are returned as-is.
    /// </summary>
    public string ResolveMediaUrl(string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return "";
        if (relativeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || relativeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return relativeUrl;
        return $"{_apiBaseUrl}{relativeUrl}";
    }

    public async Task<ApiResult<T>> GetAsync<T>(string url)
    {
        try
        {
            var response = await Client.GetAsync(url);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return new ApiResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<ApiResult<T>> PostAsync<T>(string url, object? body = null)
    {
        try
        {
            var response = body != null
                ? await Client.PostAsJsonAsync(url, body)
                : await Client.PostAsync(url, null);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return new ApiResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<ApiResult<T>> PutAsync<T>(string url, object body)
    {
        try
        {
            var response = await Client.PutAsJsonAsync(url, body);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return new ApiResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<ApiResult<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        try
        {
            var response = await Client.PostAsync(url, content);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return new ApiResult<T>(false, default, ex.Message, 0);
        }
    }

    public async Task<ApiResult<T>> DeleteAsync<T>(string url)
    {
        try
        {
            var response = await Client.DeleteAsync(url);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return new ApiResult<T>(false, default, ex.Message, 0);
        }
    }

    private static async Task<ApiResult<T>> ParseResponse<T>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (result != null)
            {
                return new ApiResult<T>(result.Success, result.Data, result.Error, statusCode);
            }
            return new ApiResult<T>(false, default, "Failed to parse response", statusCode);
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            return new ApiResult<T>(false, default, error?.Error ?? $"Request failed with status {statusCode}", statusCode);
        }
        catch
        {
            var errorText = await response.Content.ReadAsStringAsync();
            return new ApiResult<T>(false, default, errorText, statusCode);
        }
    }
}
