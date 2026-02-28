using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IBuilderRepository
{
    Task<Builder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Builder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? active = null, CancellationToken ct = default);
    Task AddAsync(Builder builder, CancellationToken ct = default);
    Task UpdateAsync(Builder builder, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
