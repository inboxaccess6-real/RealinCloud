using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Projects;
using RealEstate.Application.Features.Projects.Commands;
using RealEstate.Application.Features.Projects.Queries;

namespace RealEstate.Api.Endpoints;

public static class ProjectEndpoints
{
    public static WebApplication MapProjectEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapGet("/", async (
            int page,
            int pageSize,
            Guid? builderId,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProjectsQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                builderId));
            return Results.Ok(new ApiResponse<PagedResult<ProjectResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProjectByIdQuery(id));
            return Results.Ok(new ApiResponse<ProjectResponse>(true, result));
        });

        group.MapPost("/", async (CreateProjectRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateProjectCommand(request));
            return Results.Created($"/api/projects/{result.Id}", new ApiResponse<ProjectResponse>(true, result));
        })
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateProjectRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateProjectCommand(id, request));
            return Results.Ok(new ApiResponse<ProjectResponse>(true, result));
        })
        .RequireAuthorization();

        return app;
    }
}
