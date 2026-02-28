using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Builders;

namespace RealEstate.Admin.Services;

public class BuilderService
{
    private readonly ApiClient _api;

    public BuilderService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<BuilderResponse>>> GetBuildersAsync(
        int page = 1, int pageSize = 10, bool? active = null)
    {
        var url = $"/api/builders?page={page}&pageSize={pageSize}";
        if (active.HasValue)
            url += $"&active={active.Value}";
        return await _api.GetAsync<PagedResult<BuilderResponse>>(url);
    }

    public async Task<ApiResult<BuilderResponse>> GetBuilderByIdAsync(Guid id)
        => await _api.GetAsync<BuilderResponse>($"/api/builders/{id}");

    public async Task<ApiResult<BuilderResponse>> CreateBuilderAsync(CreateBuilderRequest request)
        => await _api.PostAsync<BuilderResponse>("/api/builders", request);

    public async Task<ApiResult<BuilderResponse>> UpdateBuilderAsync(Guid id, UpdateBuilderRequest request)
        => await _api.PutAsync<BuilderResponse>($"/api/builders/{id}", request);
}
