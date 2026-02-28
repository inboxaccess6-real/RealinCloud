using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Agents.Commands;
using RealEstate.Application.Features.Properties.Commands;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Endpoints.Admin;

public static class ApprovalEndpoints
{
    public static WebApplication MapApprovalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/approvals")
            .WithTags("Admin - Approvals")
            .RequireAuthorization();

        group.MapPost("/properties/{id:guid}/submit", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SubmitPropertyForReviewCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/properties/{id:guid}/approve", async (Guid id, IMediator mediator, ICurrentUserService currentUser) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();
            var result = await mediator.Send(new ApprovePropertyCommand(id, currentUser.UserId.Value));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/properties/{id:guid}/reject", async (Guid id, RejectRequest body, IMediator mediator, ICurrentUserService currentUser) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();
            var result = await mediator.Send(new RejectPropertyCommand(id, currentUser.UserId.Value, body.Reason));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/agents/{id:guid}/approve", async (Guid id, ApproveRequest? body, IMediator mediator, ICurrentUserService currentUser) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();
            var result = await mediator.Send(new ApproveAgentCommand(id, currentUser.UserId.Value, body?.Notes));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        group.MapPost("/agents/{id:guid}/reject", async (Guid id, RejectRequest body, IMediator mediator, ICurrentUserService currentUser) =>
        {
            if (currentUser.UserId is null)
                return Results.Unauthorized();
            var result = await mediator.Send(new RejectAgentCommand(id, currentUser.UserId.Value, body.Reason));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        return app;
    }

    public record RejectRequest(string Reason);
    public record ApproveRequest(string? Notes);
}
