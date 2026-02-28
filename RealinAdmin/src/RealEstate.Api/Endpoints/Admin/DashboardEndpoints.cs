using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Dashboard;
using RealEstate.Application.Features.Dashboard.Queries;

namespace RealEstate.Api.Endpoints.Admin;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/dashboard")
            .WithTags("Admin - Dashboard")
            .RequireAuthorization();

        group.MapGet("/metrics", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDashboardMetricsQuery());
            return Results.Ok(new ApiResponse<DashboardMetrics>(true, result));
        });

        return app;
    }
}
