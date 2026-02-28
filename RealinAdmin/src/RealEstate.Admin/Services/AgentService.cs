using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.Features.Agents.Commands;

namespace RealEstate.Admin.Services;

public class AgentService
{
    private readonly ApiClient _api;

    public AgentService(ApiClient api)
    {
        _api = api;
    }

    public async Task<ApiResult<PagedResult<AgentResponse>>> GetAgentsAsync(
        int page = 1, int pageSize = 10, string? status = null)
    {
        var url = $"/api/agents?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"&status={Uri.EscapeDataString(status)}";
        return await _api.GetAsync<PagedResult<AgentResponse>>(url);
    }

    public async Task<ApiResult<AgentResponse>> GetAgentByIdAsync(Guid id)
        => await _api.GetAsync<AgentResponse>($"/api/agents/{id}");

    public async Task<ApiResult<AgentResponse>> GetAgentByUserIdAsync(Guid userId)
        => await _api.GetAsync<AgentResponse>($"/api/agents/by-user/{userId}");

    public async Task<ApiResult<AgentResponse>> CreateAgentAsync(AdminCreateAgentRequest request)
        => await _api.PostAsync<AgentResponse>("/api/admin/agents", request);

    public async Task<ApiResult<AgentResponse>> UpdateAgentAsync(Guid id, UpdateAgentRequest request)
        => await _api.PutAsync<AgentResponse>($"/api/agents/{id}", request);

    public async Task<ApiResult<bool>> ApproveAgentAsync(Guid id, string? notes = null)
        => await _api.PostAsync<bool>($"/api/admin/approvals/agents/{id}/approve", new { Notes = notes });

    public async Task<ApiResult<bool>> RejectAgentAsync(Guid id, string reason)
        => await _api.PostAsync<bool>($"/api/admin/approvals/agents/{id}/reject", new { Reason = reason });
}
