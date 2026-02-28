using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly AppDbContext _context;

    public MediaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Media>> GetByPropertyIdAsync(Guid propertyId, CancellationToken ct = default)
    {
        return await _context.Media
            .Where(m => m.PropertyId == propertyId)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<Media?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Media.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task AddAsync(Media media, CancellationToken ct = default)
    {
        _context.Media.Add(media);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Media media, CancellationToken ct = default)
    {
        _context.Media.Remove(media);
        await _context.SaveChangesAsync(ct);
    }
}
