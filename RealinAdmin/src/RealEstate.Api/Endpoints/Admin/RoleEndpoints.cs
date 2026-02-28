using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Roles;
using RealEstate.Application.Features.Roles.Commands;
using RealEstate.Application.Features.Roles.Queries;

namespace RealEstate.Api.Endpoints.Admin;

public static class RoleEndpoints
{
    public static WebApplication MapRoleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/roles")
            .WithTags("Admin - Roles")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRolesQuery());
            return Results.Ok(new ApiResponse<List<RoleResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetRoleByIdQuery(id));
            return Results.Ok(new ApiResponse<RoleResponse>(true, result));
        });

        group.MapPost("/", async (CreateRoleRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateRoleCommand(request));
            return Results.Created($"/api/admin/roles/{result.Id}", new ApiResponse<RoleResponse>(true, result));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoleRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateRoleCommand(id, request));
            return Results.Ok(new ApiResponse<RoleResponse>(true, result));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteRoleCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPut("/{id:guid}/permissions", async (Guid id, AssignPermissionsRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new AssignPermissionsCommand(id, request));
            return Results.Ok(new ApiResponse<RoleResponse>(true, result));
        });

        return app;
    }
}
