using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Builders;
using RealEstate.Application.Features.Builders.Commands;
using RealEstate.Application.Features.Builders.Queries;

namespace RealEstate.Api.Endpoints;

public static class BuilderEndpoints
{
    public static WebApplication MapBuilderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/builders")
            .WithTags("Builders");

        group.MapGet("/", async (
            int page,
            int pageSize,
            bool? active,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBuildersQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                active));
            return Results.Ok(new ApiResponse<PagedResult<BuilderResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBuilderByIdQuery(id));
            return Results.Ok(new ApiResponse<BuilderResponse>(true, result));
        });

        group.MapPost("/", async (CreateBuilderRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateBuilderCommand(request));
            return Results.Created($"/api/builders/{result.Id}", new ApiResponse<BuilderResponse>(true, result));
        })
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateBuilderRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateBuilderCommand(id, request));
            return Results.Ok(new ApiResponse<BuilderResponse>(true, result));
        })
        .RequireAuthorization();

        return app;
    }
}
