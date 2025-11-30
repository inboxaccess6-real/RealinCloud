using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        // Only Agent, Admin, or SuperAdmin can create projects
        group.MapPost("/", CreateProject)
            .RequireAuthorization(policy => policy.RequireRole(
                RoleType.Agent.ToString(),
                RoleType.Admin.ToString(),
                RoleType.SuperAdmin.ToString()));
        
        // Public endpoint - anyone can view projects
        group.MapGet("/{id}", GetProject);
    }

    private static async Task<IResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        [FromServices] IProjectService service)
    {
        try
        {
            var project = await service.CreateProjectAsync(request);
            return Results.Ok(new ApiResponse<ProjectResponse>(true, project));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<ProjectResponse>(false, null, ex.Message));
        }
        catch (Exception)
        {
            return Results.Json(
                new ApiResponse<ProjectResponse>(false, null, "An error occurred while creating the project"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetProject(
        Guid id,
        [FromServices] IProjectService service)
    {
        var project = await service.GetProjectByIdAsync(id);
        return project != null
            ? Results.Ok(new ApiResponse<ProjectResponse>(true, project))
            : Results.NotFound(new ApiResponse<ProjectResponse>(false, null, "Project not found"));
    }
}
