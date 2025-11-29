using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public static class ModuleEndpoints
{
    public static void MapModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/modules")
            .WithTags("Admin - Modules")
            .RequireAuthorization();

        group.MapGet("/", GetAllModules)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapGet("/{id}", GetModuleById)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapPost("/", CreateModule)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapPut("/{id}", UpdateModule)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapDelete("/{id}", DeleteModule)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
    }

    private static async Task<IResult> GetAllModules(
        [FromServices] IModuleService service)
    {
        try
        {
            var modules = await service.GetAllModulesAsync();
            return Results.Ok(new ApiResponse<List<ModuleResponse>>(true, modules));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<List<ModuleResponse>>(false, null, "An error occurred while fetching modules"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetModuleById(
        Guid id,
        [FromServices] IModuleService service)
    {
        var module = await service.GetModuleByIdAsync(id);
        return module != null
            ? Results.Ok(new ApiResponse<ModuleResponse>(true, module))
            : Results.NotFound(new ApiResponse<ModuleResponse>(false, null, "Module not found"));
    }

    private static async Task<IResult> CreateModule(
        [FromBody] CreateModuleRequest request,
        [FromServices] IModuleService service)
    {
        try
        {
            var module = await service.CreateModuleAsync(request);
            return Results.Ok(new ApiResponse<ModuleResponse>(true, module));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<ModuleResponse>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<ModuleResponse>(false, null, "An error occurred while creating the module"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> UpdateModule(
        Guid id,
        [FromBody] UpdateModuleRequest request,
        [FromServices] IModuleService service)
    {
        try
        {
            var module = await service.UpdateModuleAsync(id, request);
            return module != null
                ? Results.Ok(new ApiResponse<ModuleResponse>(true, module))
                : Results.NotFound(new ApiResponse<ModuleResponse>(false, null, "Module not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<ModuleResponse>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<ModuleResponse>(false, null, "An error occurred while updating the module"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> DeleteModule(
        Guid id,
        [FromServices] IModuleService service)
    {
        try
        {
            var deleted = await service.DeleteModuleAsync(id);
            return deleted
                ? Results.Ok(new ApiResponse<object>(true, null, "Module deleted successfully"))
                : Results.NotFound(new ApiResponse<object>(false, null, "Module not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<object>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<object>(false, null, "An error occurred while deleting the module"),
                statusCode: 500
            );
        }
    }
}
