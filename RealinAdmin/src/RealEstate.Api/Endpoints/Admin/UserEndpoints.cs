using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Users;
using RealEstate.Application.Features.Users.Commands;
using RealEstate.Application.Features.Users.Queries;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Endpoints.Admin;

public static class UserEndpoints
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization();

        group.MapGet("/", async (
            int page,
            int pageSize,
            string? search,
            bool? isActive,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUsersQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                search,
                isActive));
            return Results.Ok(new ApiResponse<PagedResult<UserResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserByIdQuery(id));
            return Results.Ok(new ApiResponse<UserResponse>(true, result));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateUserCommand(id, request));
            return Results.Ok(new ApiResponse<UserResponse>(true, result));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteUserCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/{id:guid}/block", async (Guid id, IMediator mediator, ICurrentUserService currentUser) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();

            var result = await mediator.Send(new BlockUserCommand(id, currentUser.UserId.Value));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/{id:guid}/unblock", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new UnblockUserCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        return app;
    }
}
