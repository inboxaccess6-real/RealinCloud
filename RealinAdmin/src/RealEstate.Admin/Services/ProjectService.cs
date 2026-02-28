using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Projects;

namespace RealEstate.Admin.Services;

public class ProjectService
{
    private readonly ApiClient _api;

    public ProjectService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<ProjectResponse>>> GetProjectsAsync(
        int page = 1, int pageSize = 10, Guid? builderId = null)
    {
        var url = $"/api/projects?page={page}&pageSize={pageSize}";
        if (builderId.HasValue)
            url += $"&builderId={builderId.Value}";
        return await _api.GetAsync<PagedResult<ProjectResponse>>(url);
    }

    public async Task<ApiResult<ProjectResponse>> GetProjectByIdAsync(Guid id)
        => await _api.GetAsync<ProjectResponse>($"/api/projects/{id}");

    public async Task<ApiResult<ProjectResponse>> CreateProjectAsync(CreateProjectRequest request)
        => await _api.PostAsync<ProjectResponse>("/api/projects", request);

    public async Task<ApiResult<ProjectResponse>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
        => await _api.PutAsync<ProjectResponse>($"/api/projects/{id}", request);
}
