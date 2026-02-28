using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Media;
using RealEstate.Application.DTOs.Properties;

namespace RealEstate.Admin.Services;

public class PropertyService
{
    private readonly ApiClient _api;

    public PropertyService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<PropertyResponse>>> GetPropertiesAsync(
        int page = 1, int pageSize = 10, string? city = null, bool? isPublished = null, string? approvalStatus = null, Guid? agentId = null)
    {
        var url = $"/api/properties?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(city))
            url += $"&city={Uri.EscapeDataString(city)}";
        if (isPublished.HasValue)
            url += $"&isPublished={isPublished.Value}";
        if (!string.IsNullOrWhiteSpace(approvalStatus))
            url += $"&approvalStatus={Uri.EscapeDataString(approvalStatus)}";
        if (agentId.HasValue)
            url += $"&agentId={agentId.Value}";
        return await _api.GetAsync<PagedResult<PropertyResponse>>(url);
    }

    public async Task<ApiResult<PropertyResponse>> GetPropertyByIdAsync(Guid id)
        => await _api.GetAsync<PropertyResponse>($"/api/properties/{id}");

    public async Task<ApiResult<bool>> DeletePropertyAsync(Guid id)
        => await _api.DeleteAsync<bool>($"/api/properties/{id}");

    public async Task<ApiResult<PropertyResponse>> CreatePropertyAsync(CreatePropertyRequest request)
        => await _api.PostAsync<PropertyResponse>("/api/properties", request);

    public async Task<ApiResult<PropertyResponse>> UpdatePropertyAsync(Guid id, UpdatePropertyRequest request)
        => await _api.PutAsync<PropertyResponse>($"/api/properties/{id}", request);

    public async Task<ApiResult<bool>> SubmitForApprovalAsync(Guid id)
        => await _api.PostAsync<bool>($"/api/properties/{id}/submit");

    public async Task<ApiResult<bool>> ApprovePropertyAsync(Guid id)
        => await _api.PostAsync<bool>($"/api/admin/approvals/properties/{id}/approve");

    public async Task<ApiResult<bool>> RejectPropertyAsync(Guid id, string reason)
        => await _api.PostAsync<bool>($"/api/admin/approvals/properties/{id}/reject", new { Reason = reason });

    public async Task<ApiResult<List<MediaResponse>>> GetMediaAsync(Guid propertyId)
        => await _api.GetAsync<List<MediaResponse>>($"/api/properties/{propertyId}/media");

    public async Task<ApiResult<MediaResponse>> UploadMediaAsync(Guid propertyId, Stream file, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        return await _api.PostMultipartAsync<MediaResponse>($"/api/properties/{propertyId}/media", content);
    }

    public async Task<ApiResult<bool>> DeleteMediaAsync(Guid mediaId)
        => await _api.DeleteAsync<bool>($"/api/media/{mediaId}");
}
