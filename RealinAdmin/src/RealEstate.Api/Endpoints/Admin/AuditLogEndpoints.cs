using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.AuditLogs;
using RealEstate.Application.Features.AuditLogs.Queries;

namespace RealEstate.Api.Endpoints.Admin;

public static class AuditLogEndpoints
{
    public static WebApplication MapAuditLogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/audit-logs")
            .WithTags("Admin - Audit Logs")
            .RequireAuthorization();

        group.MapGet("/", async (
            int page,
            int pageSize,
            string? entityType,
            Guid? performedBy,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAuditLogsQuery(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 50,
                entityType,
                performedBy));
            return Results.Ok(new ApiResponse<PagedResult<AuditLogResponse>>(true, result));
        });

        group.MapGet("/{entityType}/{entityId:guid}", async (string entityType, Guid entityId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEntityAuditTrailQuery(entityType, entityId));
            return Results.Ok(new ApiResponse<List<AuditLogResponse>>(true, result));
        });

        return app;
    }
}
