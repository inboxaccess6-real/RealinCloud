using MediatR;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Projects.Commands;

public record UpdateProjectCommand(Guid Id, DTOs.Projects.UpdateProjectRequest Request) : IRequest<ProjectResponse?>;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ProjectResponse?>
{
    private readonly IProjectRepository _projectRepository;

    public UpdateProjectHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectResponse?> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);

        if (project == null)
            throw new NotFoundException(nameof(Domain.Entities.Project), command.Id);

        var req = command.Request;

        if (req.Name != null) project.Name = req.Name;
        if (req.ReraId != null) project.ReraId = req.ReraId;
        if (req.Address != null) project.Address = req.Address;
        if (req.Locality != null) project.Locality = req.Locality;
        if (req.City != null) project.City = req.City;
        if (req.PinCode != null) project.PinCode = req.PinCode;
        if (req.Landmark != null) project.Landmark = req.Landmark;
        if (req.Latitude != null) project.Latitude = req.Latitude;
        if (req.Longitude != null) project.Longitude = req.Longitude;
        if (req.ConstructionStatus != null) project.ConstructionStatus = req.ConstructionStatus;
        if (req.LaunchDate != null) project.LaunchDate = DateTime.SpecifyKind(req.LaunchDate.Value, DateTimeKind.Utc);
        if (req.PossessionDate != null) project.PossessionDate = DateTime.SpecifyKind(req.PossessionDate.Value, DateTimeKind.Utc);
        if (req.Status != null) project.Status = req.Status;
        if (req.TotalTowers != null) project.TotalTowers = req.TotalTowers;
        if (req.TotalUnits != null) project.TotalUnits = req.TotalUnits;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);

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
            project.Builder == null ? null : new BuilderSummary(project.Builder.Id, project.Builder.Name),
            project.CreatedByAgent == null ? null : new AgentSummary(project.CreatedByAgent.Id, project.CreatedByAgent.AgencyName, project.CreatedByAgent.Rating),
            project.Properties?.Count ?? 0);
    }
}
