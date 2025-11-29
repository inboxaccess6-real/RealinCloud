using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public interface IAgentService
{
    Task<AgentResponse?> GetAgentByIdAsync(Guid id);
    Task<AgentResponse?> GetAgentByUserIdAsync(Guid userId);
    Task<List<AgentResponse>> GetAgentsAsync(int skip = 0, int take = 50);
    Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request);
    Task<AgentResponse?> UpdateAgentAsync(Guid id, UpdateAgentRequest request);
    Task<bool> DeleteAgentAsync(Guid id);
}

public class AgentService : IAgentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AgentService> _logger;

    public AgentService(AppDbContext context, ILogger<AgentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AgentResponse?> GetAgentByIdAsync(Guid id)
    {
        var agent = await _context.Agents
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agent == null) return null;

        var propertyCount = await _context.Properties.CountAsync(p => p.AgentId == id);
        var builderCount = await _context.Builders.CountAsync(b => b.CreatedByAgentId == id);
        var projectCount = await _context.Projects.CountAsync(p => p.CreatedByAgentId == id);

        return MapToAgentResponse(agent, propertyCount, builderCount, projectCount);
    }

    public async Task<AgentResponse?> GetAgentByUserIdAsync(Guid userId)
    {
        var agent = await _context.Agents
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (agent == null) return null;

        var propertyCount = await _context.Properties.CountAsync(p => p.AgentId == agent.Id);
        var builderCount = await _context.Builders.CountAsync(b => b.CreatedByAgentId == agent.Id);
        var projectCount = await _context.Projects.CountAsync(p => p.CreatedByAgentId == agent.Id);

        return MapToAgentResponse(agent, propertyCount, builderCount, projectCount);
    }

    public async Task<List<AgentResponse>> GetAgentsAsync(int skip = 0, int take = 50)
    {
        var agents = await _context.Agents
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var responses = new List<AgentResponse>();
        foreach (var agent in agents)
        {
            var propertyCount = await _context.Properties.CountAsync(p => p.AgentId == agent.Id);
            var builderCount = await _context.Builders.CountAsync(b => b.CreatedByAgentId == agent.Id);
            var projectCount = await _context.Projects.CountAsync(p => p.CreatedByAgentId == agent.Id);
            responses.Add(MapToAgentResponse(agent, propertyCount, builderCount, projectCount));
        }

        return responses;
    }

    public async Task<AgentResponse> CreateAgentAsync(CreateAgentRequest request)
    {
        // Verify user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
            throw new InvalidOperationException($"User with ID {request.UserId} not found");

        // Check if agent already exists for this user
        var existingAgent = await _context.Agents.FirstOrDefaultAsync(a => a.UserId == request.UserId);
        if (existingAgent != null)
            throw new InvalidOperationException($"Agent already exists for user {request.UserId}");

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            LicenseNumber = request.LicenseNumber,
            AgencyName = request.AgencyName,
            ExperienceYears = request.ExperienceYears,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        return (await GetAgentByIdAsync(agent.Id))!;
    }

    public async Task<AgentResponse?> UpdateAgentAsync(Guid id, UpdateAgentRequest request)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return null;

        if (request.LicenseNumber != null) agent.LicenseNumber = request.LicenseNumber;
        if (request.AgencyName != null) agent.AgencyName = request.AgencyName;
        if (request.ExperienceYears.HasValue) agent.ExperienceYears = request.ExperienceYears;
        if (request.Rating.HasValue) agent.Rating = request.Rating;

        agent.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetAgentByIdAsync(id);
    }

    public async Task<bool> DeleteAgentAsync(Guid id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return false;

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync();
        return true;
    }

    private static AgentResponse MapToAgentResponse(Agent agent, int propertyCount, int builderCount, int projectCount)
    {
        return new AgentResponse(
            agent.Id,
            agent.UserId,
            agent.LicenseNumber,
            agent.AgencyName,
            agent.ExperienceYears,
            agent.Rating,
            agent.CreatedAt,
            agent.UpdatedAt,
            agent.User == null ? null : new UserSummary(agent.User.Id, agent.User.Name, agent.User.Email),
            propertyCount,
            builderCount,
            projectCount
        );
    }
}
