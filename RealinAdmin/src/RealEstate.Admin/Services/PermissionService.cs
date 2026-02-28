using System.Net.Http.Json;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Roles;

namespace RealEstate.Admin.Services;

public class PermissionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private Dictionary<string, RolePermissionDto>? _permissions;

    public bool IsLoaded => _permissions != null;

    public event Action? OnChanged;

    public PermissionService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task LoadPermissionsAsync(Guid roleId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthenticatedApi");
            var response = await client.GetFromJsonAsync<ApiResponse<RoleResponse>>($"/api/admin/roles/{roleId}");

            if (response?.Success == true && response.Data?.Permissions != null)
            {
                _permissions = response.Data.Permissions.ToDictionary(p => p.ModuleCode, p => p);
                OnChanged?.Invoke();
            }
        }
        catch
        {
            _permissions = null;
        }
    }

    public async Task LoadPermissionsByRoleNameAsync(string roleName)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthenticatedApi");

            // Step 1: Get all roles (permissions not included in list response)
            Console.WriteLine($"[PermissionService] Loading permissions for role: {roleName}");
            var listResponse = await client.GetFromJsonAsync<ApiResponse<List<RoleResponse>>>("/api/admin/roles");
            if (listResponse?.Success != true || listResponse.Data == null)
            {
                Console.WriteLine($"[PermissionService] Failed to load roles list. Success={listResponse?.Success}, Data is null={listResponse?.Data == null}");
                return;
            }

            Console.WriteLine($"[PermissionService] Got {listResponse.Data.Count} roles");
            var role = listResponse.Data.FirstOrDefault(r =>
                r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                Console.WriteLine($"[PermissionService] Role '{roleName}' not found in list");
                return;
            }

            // Step 2: Fetch the specific role by ID (includes permissions with module codes)
            Console.WriteLine($"[PermissionService] Fetching role detail for ID: {role.Id}");
            var detailResponse = await client.GetFromJsonAsync<ApiResponse<RoleResponse>>($"/api/admin/roles/{role.Id}");
            if (detailResponse?.Success == true && detailResponse.Data?.Permissions != null)
            {
                _permissions = detailResponse.Data.Permissions.ToDictionary(p => p.ModuleCode, p => p);
                Console.WriteLine($"[PermissionService] Loaded {_permissions.Count} permissions: {string.Join(", ", _permissions.Keys)}");
                OnChanged?.Invoke();
            }
            else
            {
                Console.WriteLine($"[PermissionService] Failed to load role detail. Success={detailResponse?.Success}, Permissions is null={detailResponse?.Data?.Permissions == null}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PermissionService] Exception: {ex.Message}");
            _permissions = null;
        }
    }

    public bool HasPermission(string moduleCode, string permission)
    {
        if (_permissions == null) return false;
        if (!_permissions.TryGetValue(moduleCode, out var perm)) return false;

        return permission.ToLower() switch
        {
            "canread" or "read" => perm.CanRead,
            "cancreate" or "create" => perm.CanCreate,
            "canupdate" or "update" => perm.CanUpdate,
            "candelete" or "delete" => perm.CanDelete,
            "canmanage" or "manage" => perm.CanManage,
            "canexport" or "export" => perm.CanExport,
            _ => false
        };
    }

    public bool HasModuleAccess(string moduleCode)
    {
        return HasPermission(moduleCode, "read");
    }

    public void ClearPermissions()
    {
        _permissions = null;
        OnChanged?.Invoke();
    }
}
