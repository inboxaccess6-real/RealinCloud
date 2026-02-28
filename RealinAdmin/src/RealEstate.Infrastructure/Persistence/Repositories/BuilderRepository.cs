using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class BuilderRepository : IBuilderRepository
{
    private readonly AppDbContext _context;

    public BuilderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Builder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Builders
            .Include(b => b.CreatedByAgent)
            .FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id, ct);
    }

    public async Task<(List<Builder> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, bool? active = null, CancellationToken ct = default)
    {
        var query = _context.Builders
            .Where(b => !b.IsDeleted);

        if (active.HasValue)
        {
            query = query.Where(b => b.Active == active.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Builder builder, CancellationToken ct = default)
    {
        _context.Builders.Add(builder);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Builder builder, CancellationToken ct = default)
    {
        _context.Builders.Update(builder);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Builders.CountAsync(b => !b.IsDeleted, ct);
    }
}
