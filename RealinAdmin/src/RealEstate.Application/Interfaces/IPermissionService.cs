namespace RealEstate.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string moduleCode, string permission);
    Task<Dictionary<string, HashSet<string>>> GetUserPermissionsAsync(Guid userId);
}
