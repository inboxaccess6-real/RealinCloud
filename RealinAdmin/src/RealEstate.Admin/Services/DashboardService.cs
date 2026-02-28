using RealEstate.Application.DTOs.Dashboard;

namespace RealEstate.Admin.Services;

public class DashboardService
{
    private readonly ApiClient _api;

    public DashboardService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<DashboardMetrics>> GetMetricsAsync()
        => await _api.GetAsync<DashboardMetrics>("/api/admin/dashboard/metrics");
}
