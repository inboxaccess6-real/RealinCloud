using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Agents.Commands;

public record ApproveAgentCommand(Guid AgentId, Guid VerifiedBy, string? Notes = null) : IRequest<bool>;

public class ApproveAgentHandler : IRequestHandler<ApproveAgentCommand, bool>
{
    private readonly IAgentRepository _agentRepository;

    public ApproveAgentHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<bool> Handle(ApproveAgentCommand command, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(command.AgentId, cancellationToken);

        if (agent == null)
            throw new NotFoundException(nameof(Domain.Entities.Agent), command.AgentId);

        agent.Status = "approved";
        agent.VerifiedBy = command.VerifiedBy;
        agent.VerifiedAt = DateTime.UtcNow;
        agent.VerificationNotes = command.Notes;
        agent.UpdatedAt = DateTime.UtcNow;

        await _agentRepository.UpdateAsync(agent, cancellationToken);

        return true;
    }
}
