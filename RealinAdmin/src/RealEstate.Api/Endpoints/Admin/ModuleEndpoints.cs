using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Modules;
using RealEstate.Application.Features.Modules.Commands;
using RealEstate.Application.Features.Modules.Queries;

namespace RealEstate.Api.Endpoints.Admin;

public static class ModuleEndpoints
{
    public static WebApplication MapModuleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/modules")
            .WithTags("Admin - Modules")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetModulesQuery());
            return Results.Ok(new ApiResponse<List<ModuleResponse>>(true, result));
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetModuleByIdQuery(id));
            return Results.Ok(new ApiResponse<ModuleResponse>(true, result));
        });

        group.MapPost("/", async (CreateModuleRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateModuleCommand(request));
            return Results.Created($"/api/admin/modules/{result.Id}", new ApiResponse<ModuleResponse>(true, result));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateModuleRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateModuleCommand(id, request));
            return Results.Ok(new ApiResponse<ModuleResponse>(true, result));
        });

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteModuleCommand(id));
            return Results.Ok(new ApiResponse<bool>(true, result));
        });

        return app;
    }
}
