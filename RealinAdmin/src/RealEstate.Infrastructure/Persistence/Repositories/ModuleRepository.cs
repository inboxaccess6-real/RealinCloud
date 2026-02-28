using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly AppDbContext _context;

    public ModuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Module>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Modules
            .OrderBy(m => m.Code)
            .ToListAsync(ct);
    }

    public async Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Modules
            .Include(m => m.RolePermissions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<Module?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Modules
            .FirstOrDefaultAsync(m => m.Code == code, ct);
    }

    public async Task AddAsync(Module module, CancellationToken ct = default)
    {
        _context.Modules.Add(module);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Module module, CancellationToken ct = default)
    {
        _context.Modules.Update(module);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Module module, CancellationToken ct = default)
    {
        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(ct);
    }
}
