using MediatR;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Features.Media.Commands;

public record DeleteMediaCommand(Guid MediaId) : IRequest<bool>;

public class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, bool>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageService _storage;

    public DeleteMediaHandler(IMediaRepository mediaRepository, IFileStorageService storage)
    {
        _mediaRepository = mediaRepository;
        _storage = storage;
    }

    public async Task<bool> Handle(DeleteMediaCommand command, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByIdAsync(command.MediaId, cancellationToken);
        if (media is null)
            throw new KeyNotFoundException($"Media {command.MediaId} not found.");

        await _storage.DeleteAsync(media.StorageBucket, media.StorageKey, cancellationToken);
        await _mediaRepository.DeleteAsync(media, cancellationToken);

        return true;
    }
}
