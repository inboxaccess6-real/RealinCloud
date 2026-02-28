using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.RoleType)
            .ToListAsync(ct);
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Module)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByRoleTypeAsync(RoleType roleType, CancellationToken ct = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleType == roleType, ct);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ClearPermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        // Use ExecuteDeleteAsync to bypass the change tracker entirely
        await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ExecuteDeleteAsync(ct);

        // Clear tracker so subsequent queries don't see stale entities
        _context.ChangeTracker.Clear();
    }

    public async Task AddPermissionsAsync(List<RolePermission> permissions, CancellationToken ct = default)
    {
        _context.RolePermissions.AddRange(permissions);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<(Role Role, int UserCount)>> GetAllWithUserCountAsync(CancellationToken ct = default)
    {
        var roles = await _context.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.RoleType)
            .ToListAsync(ct);

        var userCounts = await _context.Users
            .Where(u => !u.IsDeleted)
            .GroupBy(u => u.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, ct);

        return roles.Select(r => (r, userCounts.GetValueOrDefault(r.Id, 0))).ToList();
    }

    public async Task<int> GetUserCountAsync(Guid roleId, CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(u => u.RoleId == roleId && !u.IsDeleted, ct);
    }
}
