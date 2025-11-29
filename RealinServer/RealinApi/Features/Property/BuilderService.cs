using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public interface IBuilderService
{
    Task<BuilderResponse?> GetBuilderByIdAsync(Guid id);
    Task<List<BuilderResponse>> GetBuildersAsync(int skip = 0, int take = 50, bool? active = null);
    Task<BuilderResponse> CreateBuilderAsync(CreateBuilderRequest request);
    Task<BuilderResponse?> UpdateBuilderAsync(Guid id, UpdateBuilderRequest request);
    Task<bool> DeleteBuilderAsync(Guid id);
}

public class BuilderService : IBuilderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BuilderService> _logger;

    public BuilderService(AppDbContext context, ILogger<BuilderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BuilderResponse?> GetBuilderByIdAsync(Guid id)
    {
        var builder = await _context.Builders
            .Include(b => b.CreatedByAgent)
                .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (builder == null) return null;

        var projectCount = await _context.Projects.CountAsync(p => p.BuilderId == id);
        return MapToBuilderResponse(builder, projectCount);
    }

    public async Task<List<BuilderResponse>> GetBuildersAsync(int skip = 0, int take = 50, bool? active = null)
    {
        var query = _context.Builders
            .Include(b => b.CreatedByAgent)
            .AsQueryable();

        if (active.HasValue)
            query = query.Where(b => b.Active == active.Value);

        var builders = await query
            .OrderBy(b => b.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var responses = new List<BuilderResponse>();
        foreach (var builder in builders)
        {
            var projectCount = await _context.Projects.CountAsync(p => p.BuilderId == builder.Id);
            responses.Add(MapToBuilderResponse(builder, projectCount));
        }

        return responses;
    }

    public async Task<BuilderResponse> CreateBuilderAsync(CreateBuilderRequest request)
    {
        // Verify agent exists if provided
        if (request.CreatedByAgentId.HasValue)
        {
            var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.CreatedByAgentId.Value);
            if (!agentExists)
                throw new InvalidOperationException($"Agent with ID {request.CreatedByAgentId} not found");
        }

        var builder = new Builder
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            EstablishedYear = request.EstablishedYear,
            RegistrationNumber = request.RegistrationNumber,
            HeadquartersAddress = request.HeadquartersAddress,
            Website = request.Website,
            Active = request.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedByAgentId = request.CreatedByAgentId
        };

        _context.Builders.Add(builder);
        await _context.SaveChangesAsync();

        return (await GetBuilderByIdAsync(builder.Id))!;
    }

    public async Task<BuilderResponse?> UpdateBuilderAsync(Guid id, UpdateBuilderRequest request)
    {
        var builder = await _context.Builders.FindAsync(id);
        if (builder == null) return null;

        if (request.Name != null) builder.Name = request.Name;
        if (request.Email != null) builder.Email = request.Email;
        if (request.Phone != null) builder.Phone = request.Phone;
        if (request.EstablishedYear.HasValue) builder.EstablishedYear = request.EstablishedYear;
        if (request.RegistrationNumber != null) builder.RegistrationNumber = request.RegistrationNumber;
        if (request.HeadquartersAddress != null) builder.HeadquartersAddress = request.HeadquartersAddress;
        if (request.Website != null) builder.Website = request.Website;
        if (request.Active.HasValue) builder.Active = request.Active.Value;

        await _context.SaveChangesAsync();
        return await GetBuilderByIdAsync(id);
    }

    public async Task<bool> DeleteBuilderAsync(Guid id)
    {
        var builder = await _context.Builders.FindAsync(id);
        if (builder == null) return false;

        _context.Builders.Remove(builder);
        await _context.SaveChangesAsync();
        return true;
    }

    private static BuilderResponse MapToBuilderResponse(Builder builder, int projectCount)
    {
        return new BuilderResponse(
            builder.Id,
            builder.Name,
            builder.Email,
            builder.Phone,
            builder.EstablishedYear,
            builder.RegistrationNumber,
            builder.HeadquartersAddress,
            builder.Website,
            builder.Active,
            builder.CreatedAt,
            builder.CreatedByAgentId,
            builder.CreatedByAgent == null ? null : new AgentSummary(builder.CreatedByAgent.Id, builder.CreatedByAgent.AgencyName, builder.CreatedByAgent.Rating),
            projectCount
        );
    }
}
