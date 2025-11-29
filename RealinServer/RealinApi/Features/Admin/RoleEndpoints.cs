using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/roles")
            .WithTags("Admin - Roles")
            .RequireAuthorization();

        group.MapGet("/", GetAllRoles)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapGet("/{id}", GetRoleById)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapPost("/", CreateRole)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapPut("/{id}", UpdateRole)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapDelete("/{id}", DeleteRole)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
    }

    private static async Task<IResult> GetAllRoles(
        [FromServices] IRoleService service)
    {
        try
        {
            var roles = await service.GetAllRolesAsync();
            return Results.Ok(new ApiResponse<List<RoleResponse>>(true, roles));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<List<RoleResponse>>(false, null, "An error occurred while fetching roles"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetRoleById(
        Guid id,
        [FromServices] IRoleService service)
    {
        var role = await service.GetRoleByIdAsync(id);
        return role != null
            ? Results.Ok(new ApiResponse<RoleResponse>(true, role))
            : Results.NotFound(new ApiResponse<RoleResponse>(false, null, "Role not found"));
    }

    private static async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        [FromServices] IRoleService service)
    {
        try
        {
            var role = await service.CreateRoleAsync(request);
            return Results.Ok(new ApiResponse<RoleResponse>(true, role));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<RoleResponse>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<RoleResponse>(false, null, "An error occurred while creating the role"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        [FromServices] IRoleService service)
    {
        try
        {
            var role = await service.UpdateRoleAsync(id, request);
            return role != null
                ? Results.Ok(new ApiResponse<RoleResponse>(true, role))
                : Results.NotFound(new ApiResponse<RoleResponse>(false, null, "Role not found"));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<RoleResponse>(false, null, "An error occurred while updating the role"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> DeleteRole(
        Guid id,
        [FromServices] IRoleService service)
    {
        try
        {
            var deleted = await service.DeleteRoleAsync(id);
            return deleted
                ? Results.Ok(new ApiResponse<object>(true, null, "Role deleted successfully"))
                : Results.NotFound(new ApiResponse<object>(false, null, "Role not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<object>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<object>(false, null, "An error occurred while deleting the role"),
                statusCode: 500
            );
        }
    }
}
