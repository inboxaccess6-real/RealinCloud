using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Projects.Queries;

public record GetProjectsQuery(
    int Page = 1,
    int PageSize = 50,
    Guid? BuilderId = null
) : IRequest<PagedResult<ProjectResponse>>;

public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectResponse>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<PagedResult<ProjectResponse>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _projectRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.BuilderId,
            cancellationToken);

        var projects = items.Select(project => new ProjectResponse(
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
            project.Properties?.Count ?? 0)).ToList();

        return new PagedResult<ProjectResponse>(projects, totalCount, request.Page, request.PageSize);
    }
}
