using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Agents;
using RealEstate.Application.Features.Agents.Commands;
using RealEstate.Application.Features.Agents.Queries;

namespace RealEstate.Api.Endpoints;

public static class AgentEndpoints
{
    public static WebApplication MapAgentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agents")
            .WithTags("Agents");

        group.MapGet("/", async (
            int page,
            int pageSize,
            string? status,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAgentsQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                status));
            return Results.Ok(new ApiResponse<PagedResult<AgentResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAgentByIdQuery(id));
            return Results.Ok(new ApiResponse<AgentResponse>(true, result));
        });

        group.MapGet("/by-user/{userId:guid}", async (Guid userId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAgentByUserIdQuery(userId));
            return Results.Ok(new ApiResponse<AgentResponse?>(true, result));
        });

        group.MapPost("/", async (CreateAgentRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateAgentCommand(request));
            return Results.Created($"/api/agents/{result.Id}", new ApiResponse<AgentResponse>(true, result));
        })
        .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateAgentRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateAgentCommand(id, request));
            return Results.Ok(new ApiResponse<AgentResponse>(true, result));
        })
        .RequireAuthorization();

        // Admin: create agent (creates user + agent, auto-approved)
        app.MapPost("/api/admin/agents", async (AdminCreateAgentRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new AdminCreateAgentCommand(request));
            return Results.Created($"/api/agents/{result.Id}", new ApiResponse<AgentResponse>(true, result));
        })
        .WithTags("Admin - Agents")
        .RequireAuthorization();

        return app;
    }
}
