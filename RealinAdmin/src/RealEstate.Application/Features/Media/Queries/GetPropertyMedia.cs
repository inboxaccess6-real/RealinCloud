using MediatR;
using RealEstate.Application.DTOs.Media;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Media.Queries;

public record GetPropertyMediaQuery(Guid PropertyId) : IRequest<List<MediaResponse>>;

public class GetPropertyMediaHandler : IRequestHandler<GetPropertyMediaQuery, List<MediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageService _storage;

    public GetPropertyMediaHandler(IMediaRepository mediaRepository, IFileStorageService storage)
    {
        _mediaRepository = mediaRepository;
        _storage = storage;
    }

    public async Task<List<MediaResponse>> Handle(GetPropertyMediaQuery query, CancellationToken cancellationToken)
    {
        var mediaList = await _mediaRepository.GetByPropertyIdAsync(query.PropertyId, cancellationToken);

        var responses = new List<MediaResponse>();
        foreach (var media in mediaList)
        {
            var url = await _storage.GetPresignedUrlAsync(
                media.StorageBucket, media.StorageKey, ct: cancellationToken);
            responses.Add(new MediaResponse(media.Id, media.PropertyId, url, media.ContentType, media.UploadedAt));
        }

        return responses;
    }
}
