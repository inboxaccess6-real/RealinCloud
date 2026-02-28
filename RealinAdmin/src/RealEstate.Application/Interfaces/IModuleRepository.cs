using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IModuleRepository
{
    Task<List<Module>> GetAllAsync(CancellationToken ct = default);
    Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Module?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(Module module, CancellationToken ct = default);
    Task UpdateAsync(Module module, CancellationToken ct = default);
    Task DeleteAsync(Module module, CancellationToken ct = default);
}
