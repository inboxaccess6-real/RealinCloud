using MediatR;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Projects.Commands;

public record CreateProjectCommand(DTOs.Projects.CreateProjectRequest Request) : IRequest<ProjectResponse>;

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;

    public CreateProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectResponse> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            BuilderId = req.BuilderId,
            CreatedByAgentId = req.CreatedByAgentId,
            ReraId = req.ReraId,
            Address = req.Address,
            Locality = req.Locality,
            City = req.City,
            PinCode = req.PinCode,
            Landmark = req.Landmark,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            ConstructionStatus = req.ConstructionStatus,
            LaunchDate = req.LaunchDate.HasValue ? DateTime.SpecifyKind(req.LaunchDate.Value, DateTimeKind.Utc) : null,
            PossessionDate = req.PossessionDate.HasValue ? DateTime.SpecifyKind(req.PossessionDate.Value, DateTimeKind.Utc) : null,
            Status = req.Status,
            TotalTowers = req.TotalTowers,
            TotalUnits = req.TotalUnits,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, cancellationToken);

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
            project.IsBlocked,
            project.IsBlacklisted,
            project.IsDeleted,
            project.CreatedAt,
            project.UpdatedAt,
            project.CreatedByAgentId,
            null,
            null,
            0);
    }
}
