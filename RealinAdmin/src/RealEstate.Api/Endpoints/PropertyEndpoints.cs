using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Properties;
using RealEstate.Application.Features.Properties.Commands;
using RealEstate.Application.Features.Properties.Queries;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Endpoints;

public static class PropertyEndpoints
{
    public static WebApplication MapPropertyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/properties")
            .WithTags("Properties");

        group.MapGet("/", async (
            int page,
            int pageSize,
            string? city,
            bool? isPublished,
            string? approvalStatus,
            Guid? agentId,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPropertiesQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                city,
                isPublished,
                approvalStatus,
                agentId));
            return Results.Ok(new ApiResponse<PagedResult<PropertyResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPropertyByIdQuery(id));
            return Results.Ok(new ApiResponse<PropertyResponse>(true, result));
        });

        group.MapPost("/", async (CreatePropertyRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreatePropertyCommand(request));
            return Results.Created($"/api/properties/{result.Id}", new ApiResponse<PropertyResponse>(true, result));
        })
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdatePropertyRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdatePropertyCommand(id, request));
            return Results.Ok(new ApiResponse<PropertyResponse>(true, result));
        })
        .RequireAuthorization();

        group.MapPost("/{id:guid}/submit", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SubmitPropertyCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        })
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, ICurrentUserService currentUser) =>
        {
            var result = await mediator.Send(new DeletePropertyCommand(id, currentUser.UserId!.Value));
            return Results.Ok(new ApiResponse<bool>(true, result));
        })
        .RequireAuthorization();

        return app;
    }
}
