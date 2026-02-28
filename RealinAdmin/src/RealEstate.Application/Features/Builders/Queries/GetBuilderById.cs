using MediatR;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Builders.Queries;

public record GetBuilderByIdQuery(Guid Id) : IRequest<BuilderResponse?>;

public class GetBuilderByIdHandler : IRequestHandler<GetBuilderByIdQuery, BuilderResponse?>
{
    private readonly IBuilderRepository _builderRepository;

    public GetBuilderByIdHandler(IBuilderRepository builderRepository)
    {
        _builderRepository = builderRepository;
    }

    public async Task<BuilderResponse?> Handle(GetBuilderByIdQuery request, CancellationToken cancellationToken)
    {
        var builder = await _builderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (builder == null)
            return null;

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
            builder.IsBlocked,
            builder.IsBlacklisted,
            builder.IsDeleted,
            builder.CreatedAt,
            builder.UpdatedAt,
            builder.CreatedByAgentId,
            builder.CreatedByAgent == null ? null : new AgentSummary(builder.CreatedByAgent.Id, builder.CreatedByAgent.AgencyName, builder.CreatedByAgent.Rating),
            builder.Projects?.Count ?? 0);
    }
}
