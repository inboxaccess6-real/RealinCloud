using MediatR;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Agents.Commands;

public record CreateAgentCommand(DTOs.Agents.CreateAgentRequest Request) : IRequest<AgentResponse>;

public class CreateAgentHandler : IRequestHandler<CreateAgentCommand, AgentResponse>
{
    private readonly IAgentRepository _agentRepository;

    public CreateAgentHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgentResponse> Handle(CreateAgentCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            UserId = req.UserId,
            LicenseNumber = req.LicenseNumber,
            AgencyName = req.AgencyName,
            ExperienceYears = req.ExperienceYears,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _agentRepository.AddAsync(agent, cancellationToken);

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
            null,
            0, 0, 0);
    }
}
