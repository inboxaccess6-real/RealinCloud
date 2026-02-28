using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IMediaRepository
{
    Task<List<Media>> GetByPropertyIdAsync(Guid propertyId, CancellationToken ct = default);
    Task<Media?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Media media, CancellationToken ct = default);
    Task DeleteAsync(Media media, CancellationToken ct = default);
}
