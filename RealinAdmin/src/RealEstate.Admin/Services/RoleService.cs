using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.DTOs.Roles;

namespace RealEstate.Admin.Services;

public class RoleService
{
    private readonly ApiClient _api;

    public RoleService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<List<RoleResponse>>> GetRolesAsync()
        => await _api.GetAsync<List<RoleResponse>>("/api/admin/roles");

    public async Task<ApiResult<RoleResponse>> GetRoleByIdAsync(Guid id)
        => await _api.GetAsync<RoleResponse>($"/api/admin/roles/{id}");

    public async Task<ApiResult<RoleResponse>> AssignPermissionsAsync(Guid roleId, AssignPermissionsRequest request)
        => await _api.PutAsync<RoleResponse>($"/api/admin/roles/{roleId}/permissions", request);

    public async Task<ApiResult<List<ModuleResponse>>> GetModulesAsync()
        => await _api.GetAsync<List<ModuleResponse>>("/api/admin/modules");
}
