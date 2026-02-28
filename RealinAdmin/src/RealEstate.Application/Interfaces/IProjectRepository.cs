using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Project> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? builderId = null, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
