using MediatR;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Agents.Commands;

public record RejectAgentCommand(Guid AgentId, Guid VerifiedBy, string Reason) : IRequest<bool>;

public class RejectAgentHandler : IRequestHandler<RejectAgentCommand, bool>
{
    private readonly IAgentRepository _agentRepository;

    public RejectAgentHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<bool> Handle(RejectAgentCommand command, CancellationToken cancellationToken)
    {
        var agent = await _agentRepository.GetByIdAsync(command.AgentId, cancellationToken);

        if (agent == null)
            throw new NotFoundException(nameof(Domain.Entities.Agent), command.AgentId);

        agent.Status = "rejected";
        agent.VerifiedBy = command.VerifiedBy;
        agent.VerifiedAt = DateTime.UtcNow;
        agent.VerificationNotes = command.Reason;
        agent.UpdatedAt = DateTime.UtcNow;

        await _agentRepository.UpdateAsync(agent, cancellationToken);

        return true;
    }
}
