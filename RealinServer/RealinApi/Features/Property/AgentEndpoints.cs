using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public static class AgentEndpoints
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agents").WithTags("Agents");

        // Only authenticated users with Agent role can create their own agent profile
        group.MapPost("/", CreateAgent)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.Agent.ToString()));
        
        // Public endpoints - anyone can view agents
        group.MapGet("/{id}", GetAgent);
        group.MapGet("/user/{userId}", GetAgentByUserId);
    }

    private static async Task<IResult> CreateAgent(
        [FromBody] CreateAgentRequest request,
        [FromServices] IAgentService service,
        ClaimsPrincipal user)
    {
        try
        {
            // Get the logged-in user's ID from claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Results.Unauthorized();

            var loggedInUserId = Guid.Parse(userIdClaim);

            // Ensure user can only create agent profile for themselves
            if (request.UserId != loggedInUserId)
                return Results.Forbid();

            var agent = await service.CreateAgentAsync(request);
            return Results.Ok(new ApiResponse<AgentResponse>(true, agent));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<AgentResponse>(false, null, ex.Message));
        }
        catch (Exception)
        {
            return Results.Json(
                new ApiResponse<AgentResponse>(false, null, "An error occurred while creating the agent"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetAgent(
        Guid id,
        [FromServices] IAgentService service)
    {
        var agent = await service.GetAgentByIdAsync(id);
        return agent != null
            ? Results.Ok(new ApiResponse<AgentResponse>(true, agent))
            : Results.NotFound(new ApiResponse<AgentResponse>(false, null, "Agent not found"));
    }

    private static async Task<IResult> GetAgentByUserId(
        Guid userId,
        [FromServices] IAgentService service)
    {
        var agent = await service.GetAgentByUserIdAsync(userId);
        return agent != null
            ? Results.Ok(new ApiResponse<AgentResponse>(true, agent))
            : Results.NotFound(new ApiResponse<AgentResponse>(false, null, "Agent not found"));
    }
}
