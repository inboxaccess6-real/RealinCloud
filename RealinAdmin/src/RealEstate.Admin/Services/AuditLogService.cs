using RealEstate.Application.Common;
using RealEstate.Application.DTOs.AuditLogs;

namespace RealEstate.Admin.Services;

public class AuditLogService
{
    private readonly ApiClient _api;

    public AuditLogService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<AuditLogResponse>>> GetLogsAsync(
        int page = 1, int pageSize = 10, string? entityType = null, Guid? performedBy = null)
    {
        var url = $"/api/admin/audit-logs?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(entityType))
            url += $"&entityType={Uri.EscapeDataString(entityType)}";
        if (performedBy.HasValue)
            url += $"&performedBy={performedBy.Value}";
        return await _api.GetAsync<PagedResult<AuditLogResponse>>(url);
    }
}
