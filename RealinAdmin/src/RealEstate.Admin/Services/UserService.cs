using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Users;

namespace RealEstate.Admin.Services;

public class UserService
{
    private readonly ApiClient _api;

    public UserService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<UserResponse>>> GetUsersAsync(
        int page = 1, int pageSize = 10, string? search = null, bool? isActive = null)
    {
        var url = $"/api/admin/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (isActive.HasValue)
            url += $"&isActive={isActive.Value}";
        return await _api.GetAsync<PagedResult<UserResponse>>(url);
    }

    public async Task<ApiResult<UserResponse>> GetUserByIdAsync(Guid id)
        => await _api.GetAsync<UserResponse>($"/api/admin/users/{id}");

    public async Task<ApiResult<UserResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request)
        => await _api.PutAsync<UserResponse>($"/api/admin/users/{id}", request);

    public async Task<ApiResult<bool>> DeleteUserAsync(Guid id)
        => await _api.DeleteAsync<bool>($"/api/admin/users/{id}");

    public async Task<ApiResult<bool>> BlockUserAsync(Guid id)
        => await _api.PostAsync<bool>($"/api/admin/users/{id}/block");

    public async Task<ApiResult<bool>> UnblockUserAsync(Guid id)
        => await _api.PostAsync<bool>($"/api/admin/users/{id}/unblock");
}
