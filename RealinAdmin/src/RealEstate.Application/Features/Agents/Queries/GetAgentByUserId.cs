using MediatR;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Agents.Queries;

public record GetAgentByUserIdQuery(Guid UserId) : IRequest<AgentResponse?>;

public class GetAgentByUserIdHandler : IRequestHandler<GetAgentByUserIdQuery, AgentResponse?>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentByUserIdHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgentResponse?> Handle(GetAgentByUserIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (agent is null) return null;

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
            agent.User is not null
                ? new UserSummary(agent.User.Id, agent.User.Name, agent.User.Email, agent.User.PhoneNumber)
                : null,
            agent.Properties?.Count ?? 0,
            agent.CreatedBuilders?.Count ?? 0,
            agent.CreatedProjects?.Count ?? 0);
    }
}
