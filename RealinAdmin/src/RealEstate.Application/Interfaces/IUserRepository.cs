using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<(List<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, bool? isActive = null, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}
