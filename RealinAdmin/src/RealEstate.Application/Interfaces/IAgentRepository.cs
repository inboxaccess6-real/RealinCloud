using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IAgentRepository
{
    Task<Agent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Agent?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<(List<Agent> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? status = null, CancellationToken ct = default);
    Task AddAsync(Agent agent, CancellationToken ct = default);
    Task UpdateAsync(Agent agent, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountByStatusAsync(string status, CancellationToken ct = default);
}
