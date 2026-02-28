using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AppDbContext _context;

    public AgentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Agent?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Agents
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == id, ct);
    }

    public async Task<Agent?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Agents
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => !a.IsDeleted && a.UserId == userId, ct);
    }

    public async Task<(List<Agent> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? status = null, CancellationToken ct = default)
    {
        var query = _context.Agents
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Agent agent, CancellationToken ct = default)
    {
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Agent agent, CancellationToken ct = default)
    {
        _context.Agents.Update(agent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Agents.CountAsync(a => !a.IsDeleted, ct);
    }

    public async Task<int> CountByStatusAsync(string status, CancellationToken ct = default)
    {
        return await _context.Agents
            .CountAsync(a => !a.IsDeleted && a.Status == status, ct);
    }
}
