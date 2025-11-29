using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public static class BuilderEndpoints
{
    public static void MapBuilderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/builders").WithTags("Builders");

        // Only Agent, Admin, or SuperAdmin can create builders
        group.MapPost("/", CreateBuilder)
            .RequireAuthorization(policy => policy.RequireRole(
                RoleType.Agent.ToString(),
                RoleType.Admin.ToString(),
                RoleType.SuperAdmin.ToString()));
        
        // Public endpoint - anyone can view builders
        group.MapGet("/{id}", GetBuilder);
    }

    private static async Task<IResult> CreateBuilder(
        [FromBody] CreateBuilderRequest request,
        [FromServices] IBuilderService service)
    {
        try
        {
            var builder = await service.CreateBuilderAsync(request);
            return Results.Ok(new ApiResponse<BuilderResponse>(true, builder));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<BuilderResponse>(false, null, ex.Message));
        }
        catch (Exception)
        {
            return Results.Json(
                new ApiResponse<BuilderResponse>(false, null, "An error occurred while creating the builder"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetBuilder(
        Guid id,
        [FromServices] IBuilderService service)
    {
        var builder = await service.GetBuilderByIdAsync(id);
        return builder != null
            ? Results.Ok(new ApiResponse<BuilderResponse>(true, builder))
            : Results.NotFound(new ApiResponse<BuilderResponse>(false, null, "Builder not found"));
    }
}
