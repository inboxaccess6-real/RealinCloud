using MediatR;
using RealEstate.Application.Common;
using RealEstate.Application.DTOs.Media;
using RealEstate.Application.Features.Media.Commands;
using RealEstate.Application.Features.Media.Queries;

namespace RealEstate.Api.Endpoints;

public static class MediaEndpoints
{
    public static WebApplication MapMediaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Media");

        group.MapGet("/properties/{propertyId:guid}/media", async (Guid propertyId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPropertyMediaQuery(propertyId));
            return Results.Ok(new ApiResponse<List<MediaResponse>>(true, result));
        });

        group.MapPost("/properties/{propertyId:guid}/media", async (Guid propertyId, IFormFile file, IMediator mediator) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await mediator.Send(new UploadMediaCommand(
                propertyId, stream, file.FileName, file.ContentType));
            return Results.Created($"/api/media/{result.Id}", new ApiResponse<MediaResponse>(true, result));
        })
        .RequireAuthorization()
        .DisableAntiforgery();

        group.MapDelete("/media/{mediaId:guid}", async (Guid mediaId, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteMediaCommand(mediaId));
            return Results.Ok(new ApiResponse<bool>(true, result));
        })
        .RequireAuthorization();

        return app;
    }
}
