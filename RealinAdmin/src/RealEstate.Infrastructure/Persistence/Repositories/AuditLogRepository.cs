using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityType = null, Guid? performedBy = null,
        CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (performedBy.HasValue)
        {
            query = query.Where(a => a.PerformedBy == performedBy.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(a => a.PerformedByUser)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<AuditLog>> GetByEntityAsync(
        string entityType, Guid entityId, CancellationToken ct = default)
    {
        return await _context.AuditLogs
            .Include(a => a.PerformedByUser)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}
