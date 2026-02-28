using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync(CancellationToken ct = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Role?> GetByRoleTypeAsync(RoleType roleType, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Role role, CancellationToken ct = default);
    Task UpdateAsync(Role role, CancellationToken ct = default);
    Task DeleteAsync(Role role, CancellationToken ct = default);
    Task ClearPermissionsAsync(Guid roleId, CancellationToken ct = default);
    Task AddPermissionsAsync(List<RolePermission> permissions, CancellationToken ct = default);
    Task<List<(Role Role, int UserCount)>> GetAllWithUserCountAsync(CancellationToken ct = default);
    Task<int> GetUserCountAsync(Guid roleId, CancellationToken ct = default);
}
