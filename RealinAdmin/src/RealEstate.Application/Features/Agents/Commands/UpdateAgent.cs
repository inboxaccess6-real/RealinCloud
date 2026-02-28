using MediatR;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Agents.Commands;

public record UpdateAgentCommand(Guid Id, DTOs.Agents.UpdateAgentRequest Request) : IRequest<AgentResponse?>;

public class UpdateAgentHandler : IRequestHandler<UpdateAgentCommand, AgentResponse?>
{
    private readonly IAgentRepository _agentRepository;

    public UpdateAgentHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgentResponse?> Handle(UpdateAgentCommand command, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(command.Id, cancellationToken);

        if (agent == null)
            throw new NotFoundException(nameof(Domain.Entities.Agent), command.Id);

        var req = command.Request;

        if (req.LicenseNumber != null) agent.LicenseNumber = req.LicenseNumber;
        if (req.AgencyName != null) agent.AgencyName = req.AgencyName;
        if (req.ExperienceYears != null) agent.ExperienceYears = req.ExperienceYears;
        if (req.Rating != null) agent.Rating = req.Rating;
        agent.UpdatedAt = DateTime.UtcNow;

        await _agentRepository.UpdateAsync(agent, cancellationToken);

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
