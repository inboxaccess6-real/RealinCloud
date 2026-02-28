using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Property> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? city = null, bool? isPublished = null, string? approvalStatus = null, Guid? agentId = null, CancellationToken ct = default);
    Task AddAsync(Property property, CancellationToken ct = default);
    Task UpdateAsync(Property property, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountByApprovalStatusAsync(string status, CancellationToken ct = default);
    Task<int> CountFlaggedAsync(CancellationToken ct = default);
}
