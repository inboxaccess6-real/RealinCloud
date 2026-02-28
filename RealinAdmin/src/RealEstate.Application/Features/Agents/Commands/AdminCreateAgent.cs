using MediatR;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Agents.Commands;

public record AdminCreateAgentRequest(
    string Name,
    string? Email = null,
    string? PhoneNumber = null,
    string? LicenseNumber = null,
    string? AgencyName = null,
    int? ExperienceYears = null
);

public record AdminCreateAgentCommand(AdminCreateAgentRequest Request) : IRequest<AgentResponse>;

public class AdminCreateAgentHandler : IRequestHandler<AdminCreateAgentCommand, AgentResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IRoleRepository _roleRepository;

    public AdminCreateAgentHandler(
        IUserRepository userRepository,
        IAgentRepository agentRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _agentRepository = agentRepository;
        _roleRepository = roleRepository;
    }

    public async Task<AgentResponse> Handle(AdminCreateAgentCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        if (string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(req.PhoneNumber))
            throw new BusinessRuleException("Either email or phone number is required.");

        // Check if user already exists by email or phone
        User? user = null;
        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            user = await _userRepository.GetByEmailAsync(req.Email, cancellationToken);
        }
        if (user is null && !string.IsNullOrWhiteSpace(req.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneAsync(req.PhoneNumber, cancellationToken);
        }

        // Get Agent role
        var agentRole = await _roleRepository.GetByRoleTypeAsync(RoleType.Agent, cancellationToken)
            ?? throw new BusinessRuleException("Agent role not found. Please create it first.");

        if (user is not null)
        {
            // Check if already an agent
            var existingAgent = await _agentRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (existingAgent is not null)
                throw new BusinessRuleException("This user is already registered as an agent.");

            // Update user details and upgrade role to Agent
            user.Name = req.Name;
            if (!string.IsNullOrWhiteSpace(req.Email) && string.IsNullOrWhiteSpace(user.Email))
                user.Email = req.Email;
            if (!string.IsNullOrWhiteSpace(req.PhoneNumber) && string.IsNullOrWhiteSpace(user.PhoneNumber))
                user.PhoneNumber = req.PhoneNumber;
            user.RoleId = agentRole.Id;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }
        else
        {
            // Create new user with Agent role
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber,
                Provider = AuthProvider.Otp,
                RoleId = agentRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, cancellationToken);
        }

        // Create agent record — auto-approved since admin is creating
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LicenseNumber = req.LicenseNumber,
            AgencyName = req.AgencyName,
            ExperienceYears = req.ExperienceYears,
            Status = "approved",
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
            new UserSummary(user.Id, user.Name, user.Email, user.PhoneNumber),
            0, 0, 0);
    }
}
