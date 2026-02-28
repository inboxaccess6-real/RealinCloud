using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Agents.Queries;

public record GetAgentsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Status = null
) : IRequest<PagedResult<AgentResponse>>;

public class GetAgentsHandler : IRequestHandler<GetAgentsQuery, PagedResult<AgentResponse>>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentsHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<PagedResult<AgentResponse>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _agentRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var agents = items.Select(agent => new AgentResponse(
            agent.Id,
            agent.UserId,
            agent.LicenseNumber,
            agent.AgencyName,
            agent.ExperienceYears,
            agent.Rating,
            agent.Status,
            agent.VerificationNotes,
            agent.IsBlocked,
            agent.IsBlacklisted,
            agent.IsDeleted,
            agent.CreatedAt,
            agent.UpdatedAt,
            agent.User == null ? null : new UserSummary(agent.User.Id, agent.User.Name, agent.User.Email),
            0, 0, 0)).ToList();

        return new PagedResult<AgentResponse>(agents, totalCount, request.Page, request.PageSize);
    }
}
