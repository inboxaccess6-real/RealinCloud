using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken ct = default);
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? entityType = null, Guid? performedBy = null, CancellationToken ct = default);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}
