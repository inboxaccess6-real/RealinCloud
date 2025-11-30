using Microsoft.AspNetCore.Mvc;
using RealinApi.Common;
using RealinApi.Data.Entities;
using RealinApi.Features.Admin.Models;

namespace RealinApi.Features.Admin;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization();

        group.MapGet("/", GetAllUsers)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapGet("/{id}", GetUserById)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapPut("/{id}", UpdateUser)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
        
        group.MapDelete("/{id}", DeleteUser)
            .RequireAuthorization(policy => policy.RequireRole(RoleType.SuperAdmin.ToString()));
    }

    private static async Task<IResult> GetAllUsers(
        [FromServices] IUserService service,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        try
        {
            var users = await service.GetAllUsersAsync(skip, take);
            return Results.Ok(new ApiResponse<List<UserResponse>>(true, users));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<List<UserResponse>>(false, null, "An error occurred while fetching users"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        [FromServices] IUserService service)
    {
        var user = await service.GetUserByIdAsync(id);
        return user != null
            ? Results.Ok(new ApiResponse<UserResponse>(true, user))
            : Results.NotFound(new ApiResponse<UserResponse>(false, null, "User not found"));
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] IUserService service)
    {
        try
        {
            var user = await service.UpdateUserAsync(id, request);
            return user != null
                ? Results.Ok(new ApiResponse<UserResponse>(true, user))
                : Results.NotFound(new ApiResponse<UserResponse>(false, null, "User not found"));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ApiResponse<UserResponse>(false, null, ex.Message));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<UserResponse>(false, null, "An error occurred while updating the user"),
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        [FromServices] IUserService service)
    {
        try
        {
            var deleted = await service.DeleteUserAsync(id);
            return deleted
                ? Results.Ok(new ApiResponse<object>(true, null, "User deleted successfully"))
                : Results.NotFound(new ApiResponse<object>(false, null, "User not found"));
        }
        catch (Exception ex)
        {
            return Results.Json(
                new ApiResponse<object>(false, null, "An error occurred while deleting the user"),
                statusCode: 500
            );
        }
    }
}
