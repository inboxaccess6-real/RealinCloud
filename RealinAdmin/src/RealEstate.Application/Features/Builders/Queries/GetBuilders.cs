using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Builders.Queries;

public record GetBuildersQuery(
    int Page = 1,
    int PageSize = 50,
    bool? Active = null
) : IRequest<PagedResult<BuilderResponse>>;

public class GetBuildersHandler : IRequestHandler<GetBuildersQuery, PagedResult<BuilderResponse>>
{
    private readonly IBuilderRepository _builderRepository;

    public GetBuildersHandler(IBuilderRepository builderRepository)
    {
        _builderRepository = builderRepository;
    }

    public async Task<PagedResult<BuilderResponse>> Handle(GetBuildersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _builderRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Active,
            cancellationToken);

        var builders = items.Select(builder => new BuilderResponse(
            builder.Id,
            builder.Name,
            builder.Email,
            builder.Phone,
            builder.EstablishedYear,
            builder.RegistrationNumber,
            builder.HeadquartersAddress,
            builder.Website,
            builder.Active,
            builder.IsBlocked,
            builder.IsBlacklisted,
            builder.IsDeleted,
            builder.CreatedAt,
            builder.UpdatedAt,
            builder.CreatedByAgentId,
            builder.CreatedByAgent == null ? null : new AgentSummary(builder.CreatedByAgent.Id, builder.CreatedByAgent.AgencyName, builder.CreatedByAgent.Rating),
            builder.Projects?.Count ?? 0)).ToList();

        return new PagedResult<BuilderResponse>(builders, totalCount, request.Page, request.PageSize);
    }
}
