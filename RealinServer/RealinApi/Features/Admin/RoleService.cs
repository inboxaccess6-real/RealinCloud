using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllRolesAsync();
    Task<RoleResponse?> GetRoleByIdAsync(Guid id);
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleResponse?> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(Guid id);
}

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleService> _logger;

    public RoleService(AppDbContext context, ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RoleResponse>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.RoleType)
            .ToListAsync();

        var responses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var userCount = await _context.Users.CountAsync(u => u.RoleId == role.Id);
            responses.Add(MapToRoleResponse(role, userCount));
        }

        return responses;
    }

    public async Task<RoleResponse?> GetRoleByIdAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return null;

        var userCount = await _context.Users.CountAsync(u => u.RoleId == id);
        return MapToRoleResponse(role, userCount);
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
    {
        var isValidRole = request.Name.TryParseRoleType(out var roleType);
        if (!isValidRole)
            throw new InvalidOperationException("Invalid role type");
        
        // Check if role with same RoleType already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleType == roleType);
        
        if (existingRole != null)
            throw new InvalidOperationException($"Role with type {roleType} already exists");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleType = roleType,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return MapToRoleResponse(role, 0);
    }

    public async Task<RoleResponse?> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return null;

        if (request.Name != null) role.Name = request.Name;
        if (request.Description != null) role.Description = request.Description;
        if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;

        role.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        var userCount = await _context.Users.CountAsync(u => u.RoleId == id);
        return MapToRoleResponse(role, userCount);
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return false;

        // Check if any users have this role
        var hasUsers = await _context.Users.AnyAsync(u => u.RoleId == id);
        if (hasUsers)
            throw new InvalidOperationException("Cannot delete role that has assigned users");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    private static RoleResponse MapToRoleResponse(Role role, int userCount)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            userCount
        );
    }
}
