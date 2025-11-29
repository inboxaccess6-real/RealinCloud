using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Property.Models;

namespace RealinApi.Features.Property;

public static class PropertyEndpoints
{
    public static void MapPropertyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/properties").WithTags("Properties");

        // Only Agent, Admin, or SuperAdmin can create properties
        group.MapPost("/", CreateProperty)
            .RequireAuthorization(policy => policy.RequireRole(
                RoleType.Agent.ToString(),
                RoleType.Admin.ToString(),
                RoleType.SuperAdmin.ToString()));
        
        // Public endpoint - anyone can view properties
        group.MapGet("/{id}", GetProperty);
    }

    private static async Task<IResult> CreateProperty(
        [FromBody] CreatePropertyRequest request,
        [FromServices] IPropertyService service)
    {
        try
        {
            var property = await service.CreatePropertyAsync(request);
            return Results.Ok(new ApiResponse<PropertyResponse>(true, property));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<PropertyResponse>(false, null, ex.Message));
        }
        catch (Exception)
        {
            return Results.Json(
                new ApiResponse<PropertyResponse>(false, null, "An error occurred while creating the property"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetProperty(
        Guid id,
        [FromServices] IPropertyService service)
    {
        var property = await service.GetPropertyByIdAsync(id);
        return property != null
            ? Results.Ok(new ApiResponse<PropertyResponse>(true, property))
            : Results.NotFound(new ApiResponse<PropertyResponse>(false, null, "Property not found"));
    }
}
