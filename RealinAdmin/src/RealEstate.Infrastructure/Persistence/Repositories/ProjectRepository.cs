using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Builder)
            .Include(p => p.CreatedByAgent)
            .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id, ct);
    }

    public async Task<(List<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? builderId = null, CancellationToken ct = default)
    {
        var query = _context.Projects
            .Where(p => !p.IsDeleted);

        if (builderId.HasValue)
        {
            query = query.Where(p => p.BuilderId == builderId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(p => p.Builder)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Projects.CountAsync(p => !p.IsDeleted, ct);
    }
}
