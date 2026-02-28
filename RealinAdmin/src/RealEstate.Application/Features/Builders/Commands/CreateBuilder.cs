using MediatR;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Builders.Commands;

public record CreateBuilderCommand(DTOs.Builders.CreateBuilderRequest Request) : IRequest<BuilderResponse>;

public class CreateBuilderHandler : IRequestHandler<CreateBuilderCommand, BuilderResponse>
{
    private readonly IBuilderRepository _builderRepository;

    public CreateBuilderHandler(IBuilderRepository builderRepository)
    {
        _builderRepository = builderRepository;
    }

    public async Task<BuilderResponse> Handle(CreateBuilderCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        var builder = new Builder
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            EstablishedYear = req.EstablishedYear,
            RegistrationNumber = req.RegistrationNumber,
            HeadquartersAddress = req.HeadquartersAddress,
            Website = req.Website,
            Active = req.Active,
            CreatedByAgentId = req.CreatedByAgentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _builderRepository.AddAsync(builder, cancellationToken);

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
            null,
            0);
    }
}
