using Microsoft.EntityFrameworkCore;
using RealinApi.Data;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public interface IProjectService
{
    Task<ProjectResponse?> GetProjectByIdAsync(Guid id);
    Task<List<ProjectResponse>> GetProjectsAsync(int skip = 0, int take = 50, Guid? builderId = null);
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
    Task<ProjectResponse?> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(Guid id);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Builder)
            .Include(p => p.CreatedByAgent)
                .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return null;

        var propertyCount = await _context.Properties.CountAsync(p => p.ProjectId == id);
        return MapToProjectResponse(project, propertyCount);
    }

    public async Task<List<ProjectResponse>> GetProjectsAsync(int skip = 0, int take = 50, Guid? builderId = null)
    {
        var query = _context.Projects
            .Include(p => p.Builder)
            .Include(p => p.CreatedByAgent)
            .AsQueryable();

        if (builderId.HasValue)
            query = query.Where(p => p.BuilderId == builderId.Value);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var responses = new List<ProjectResponse>();
        foreach (var project in projects)
        {
            var propertyCount = await _context.Properties.CountAsync(p => p.ProjectId == project.Id);
            responses.Add(MapToProjectResponse(project, propertyCount));
        }

        return responses;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
    {
        // Verify builder exists
        var builderExists = await _context.Builders.AnyAsync(b => b.Id == request.BuilderId);
        if (!builderExists)
            throw new InvalidOperationException($"Builder with ID {request.BuilderId} not found");

        // Verify agent exists if provided
        if (request.CreatedByAgentId.HasValue)
        {
            var agentExists = await _context.Agents.AnyAsync(a => a.Id == request.CreatedByAgentId.Value);
            if (!agentExists)
                throw new InvalidOperationException($"Agent with ID {request.CreatedByAgentId} not found");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BuilderId = request.BuilderId,
            CreatedByAgentId = request.CreatedByAgentId,
            ReraId = request.ReraId,
            Address = request.Address,
            Locality = request.Locality,
            City = request.City,
            PinCode = request.PinCode,
            Landmark = request.Landmark,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ConstructionStatus = request.ConstructionStatus,
            LaunchDate = request.LaunchDate,
            PossessionDate = request.PossessionDate,
            Status = request.Status,
            TotalTowers = request.TotalTowers,
            TotalUnits = request.TotalUnits,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return (await GetProjectByIdAsync(project.Id))!;
    }

    public async Task<ProjectResponse?> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return null;

        if (request.Name != null) project.Name = request.Name;
        if (request.ReraId != null) project.ReraId = request.ReraId;
        if (request.Address != null) project.Address = request.Address;
        if (request.Locality != null) project.Locality = request.Locality;
        if (request.City != null) project.City = request.City;
        if (request.PinCode != null) project.PinCode = request.PinCode;
        if (request.Landmark != null) project.Landmark = request.Landmark;
        if (request.Latitude.HasValue) project.Latitude = request.Latitude;
        if (request.Longitude.HasValue) project.Longitude = request.Longitude;
        if (request.ConstructionStatus != null) project.ConstructionStatus = request.ConstructionStatus;
        if (request.LaunchDate.HasValue) project.LaunchDate = request.LaunchDate;
        if (request.PossessionDate.HasValue) project.PossessionDate = request.PossessionDate;
        if (request.Status != null) project.Status = request.Status;
        if (request.TotalTowers.HasValue) project.TotalTowers = request.TotalTowers;
        if (request.TotalUnits.HasValue) project.TotalUnits = request.TotalUnits;

        await _context.SaveChangesAsync();
        return await GetProjectByIdAsync(id);
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProjectResponse MapToProjectResponse(Project project, int propertyCount)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.BuilderId,
            project.ReraId,
            project.Address,
            project.Locality,
            project.City,
            project.PinCode,
            project.Landmark,
            project.Latitude,
            project.Longitude,
            project.ConstructionStatus,
            project.LaunchDate,
            project.PossessionDate,
            project.Status,
            project.TotalTowers,
            project.TotalUnits,
            project.CreatedAt,
            project.CreatedByAgentId,
            project.Builder == null ? null : new BuilderSummary(project.Builder.Id, project.Builder.Name),
            project.CreatedByAgent == null ? null : new AgentSummary(project.CreatedByAgent.Id, project.CreatedByAgent.AgencyName, project.CreatedByAgent.Rating),
            propertyCount
        );
    }
}
