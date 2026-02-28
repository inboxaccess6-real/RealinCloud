using MediatR;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.DTOs.Common;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Features.Builders.Commands;

public record UpdateBuilderCommand(Guid Id, DTOs.Builders.UpdateBuilderRequest Request) : IRequest<BuilderResponse?>;

public class UpdateBuilderHandler : IRequestHandler<UpdateBuilderCommand, BuilderResponse?>
{
    private readonly IBuilderRepository _builderRepository;

    public UpdateBuilderHandler(IBuilderRepository builderRepository)
    {
        _builderRepository = builderRepository;
    }

    public async Task<BuilderResponse?> Handle(UpdateBuilderCommand command, CancellationToken cancellationToken)
    {
        var builder = await _builderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (builder == null)
            throw new NotFoundException(nameof(Domain.Entities.Builder), command.Id);

        var req = command.Request;

        if (req.Name != null) builder.Name = req.Name;
        if (req.Email != null) builder.Email = req.Email;
        if (req.Phone != null) builder.Phone = req.Phone;
        if (req.EstablishedYear != null) builder.EstablishedYear = req.EstablishedYear;
        if (req.RegistrationNumber != null) builder.RegistrationNumber = req.RegistrationNumber;
        if (req.HeadquartersAddress != null) builder.HeadquartersAddress = req.HeadquartersAddress;
        if (req.Website != null) builder.Website = req.Website;
        if (req.Active != null) builder.Active = req.Active.Value;
        builder.UpdatedAt = DateTime.UtcNow;

        await _builderRepository.UpdateAsync(builder, cancellationToken);

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
