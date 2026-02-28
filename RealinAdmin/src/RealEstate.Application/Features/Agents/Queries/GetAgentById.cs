using MediatR;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Agents.Queries;

public record GetAgentByIdQuery(Guid Id) : IRequest<AgentResponse?>;

public class GetAgentByIdHandler : IRequestHandler<GetAgentByIdQuery, AgentResponse?>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentByIdHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgentResponse?> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (agent == null)
            return null;

        return new AgentResponse(
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
            agent.Properties?.Count ?? 0,
            agent.CreatedBuilders?.Count ?? 0,
            agent.CreatedProjects?.Count ?? 0);
    }
}
