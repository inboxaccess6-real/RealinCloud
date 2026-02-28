using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly AppDbContext _context;

    public PropertyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Properties
            .Include(p => p.Agent)
                .ThenInclude(a => a.User)
            .Include(p => p.Project)
            .Include(p => p.MediaFiles)
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id, ct);
    }

    public async Task<(List<Property> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? city = null, bool? isPublished = null,
        string? approvalStatus = null, Guid? agentId = null, CancellationToken ct = default)
    {
        var query = _context.Properties
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(p => p.City == city);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(p => p.IsPublished == isPublished.Value);
        }

        if (!string.IsNullOrWhiteSpace(approvalStatus))
        {
            query = query.Where(p => p.ApprovalStatus == approvalStatus);
        }

        if (agentId.HasValue)
        {
            query = query.Where(p => p.AgentId == agentId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(p => p.Agent)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Property property, CancellationToken ct = default)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Property property, CancellationToken ct = default)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Properties.CountAsync(p => !p.IsDeleted, ct);
    }

    public async Task<int> CountByApprovalStatusAsync(string status, CancellationToken ct = default)
    {
        return await _context.Properties
            .CountAsync(p => !p.IsDeleted && p.ApprovalStatus == status, ct);
    }

    public async Task<int> CountFlaggedAsync(CancellationToken ct = default)
    {
        return await _context.Properties
            .CountAsync(p => !p.IsDeleted && p.IsFlagged, ct);
    }
}
