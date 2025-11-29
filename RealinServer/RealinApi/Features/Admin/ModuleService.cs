using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public interface IModuleService
{
    Task<List<ModuleResponse>> GetAllModulesAsync();
    Task<ModuleResponse?> GetModuleByIdAsync(Guid id);
    Task<ModuleResponse> CreateModuleAsync(CreateModuleRequest request);
    Task<ModuleResponse?> UpdateModuleAsync(Guid id, UpdateModuleRequest request);
    Task<bool> DeleteModuleAsync(Guid id);
}

public class ModuleService : IModuleService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(AppDbContext context, ILogger<ModuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ModuleResponse>> GetAllModulesAsync()
    {
        var modules = await _context.Modules
            .OrderBy(m => m.Code)
            .ToListAsync();

        var responses = new List<ModuleResponse>();
        foreach (var module in modules)
        {
            var permissionCount = await _context.RolePermissions.CountAsync(rp => rp.ModuleId == module.Id);
            responses.Add(MapToModuleResponse(module, permissionCount));
        }

        return responses;
    }

    public async Task<ModuleResponse?> GetModuleByIdAsync(Guid id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null) return null;

        var permissionCount = await _context.RolePermissions.CountAsync(rp => rp.ModuleId == id);
        return MapToModuleResponse(module, permissionCount);
    }

    public async Task<ModuleResponse> CreateModuleAsync(CreateModuleRequest request)
    {
        // Check if module with same code already exists
        var existingModule = await _context.Modules
            .FirstOrDefaultAsync(m => m.Code.ToUpper() == request.Code.ToUpper());
        
        if (existingModule != null)
            throw new InvalidOperationException($"Module with code {request.Code} already exists");

        var module = new Module
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        return MapToModuleResponse(module, 0);
    }

    public async Task<ModuleResponse?> UpdateModuleAsync(Guid id, UpdateModuleRequest request)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null) return null;

        if (request.Code != null)
        {
            var codeExists = await _context.Modules
                .AnyAsync(m => m.Code.ToUpper() == request.Code.ToUpper() && m.Id != id);
            if (codeExists)
                throw new InvalidOperationException($"Module with code {request.Code} already exists");
            
            module.Code = request.Code.ToUpper();
        }
        
        if (request.Name != null) module.Name = request.Name;
        if (request.Description != null) module.Description = request.Description;
        if (request.IsActive.HasValue) module.IsActive = request.IsActive.Value;

        module.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        var permissionCount = await _context.RolePermissions.CountAsync(rp => rp.ModuleId == id);
        return MapToModuleResponse(module, permissionCount);
    }

    public async Task<bool> DeleteModuleAsync(Guid id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null) return false;

        // Check if any role permissions exist for this module
        var hasPermissions = await _context.RolePermissions.AnyAsync(rp => rp.ModuleId == id);
        if (hasPermissions)
            throw new InvalidOperationException("Cannot delete module that has assigned permissions");

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ModuleResponse MapToModuleResponse(Module module, int permissionCount)
    {
        return new ModuleResponse(
            module.Id,
            module.Code,
            module.Name,
            module.Description,
            module.IsActive,
            module.CreatedAt,
            module.UpdatedAt,
            permissionCount
        );
    }
}
