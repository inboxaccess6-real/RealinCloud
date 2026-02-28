using MediatR;
using RealEstate.Application.DTOs.Media;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Media.Commands;

public record UploadMediaCommand(
    Guid PropertyId,
    Stream File,
    string FileName,
    string ContentType
) : IRequest<MediaResponse>;

public class UploadMediaHandler : IRequestHandler<UploadMediaCommand, MediaResponse>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageService _storage;

    public UploadMediaHandler(
        IPropertyRepository propertyRepository,
        IMediaRepository mediaRepository,
        IFileStorageService storage)
    {
        _propertyRepository = propertyRepository;
        _mediaRepository = mediaRepository;
        _storage = storage;
    }

    public async Task<MediaResponse> Handle(UploadMediaCommand command, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(command.PropertyId, cancellationToken);
        if (property is null)
            throw new KeyNotFoundException($"Property {command.PropertyId} not found.");

        var (bucket, key) = await _storage.UploadAsync(
            command.File, command.FileName, command.ContentType, cancellationToken);

        var url = await _storage.GetPresignedUrlAsync(bucket, key, ct: cancellationToken);

        var media = new Domain.Entities.Media
        {
            Id = Guid.NewGuid(),
            PropertyId = command.PropertyId,
            StorageBucket = bucket,
            StorageKey = key,
            ContentType = command.ContentType,
            Url = url,
            UploadedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(media, cancellationToken);

        return new MediaResponse(media.Id, media.PropertyId, url, media.ContentType, media.UploadedAt);
    }
}
