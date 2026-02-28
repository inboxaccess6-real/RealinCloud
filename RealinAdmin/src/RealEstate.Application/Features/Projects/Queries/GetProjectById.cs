using MediatR;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectResponse?>;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponse?>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectResponse?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

        if (project == null)
            return null;

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
