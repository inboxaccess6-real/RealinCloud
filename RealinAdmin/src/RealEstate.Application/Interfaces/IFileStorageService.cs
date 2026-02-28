namespace RealEstate.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string Bucket, string Key)> UploadAsync(Stream file, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string bucket, string key, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string bucket, string key, TimeSpan? expiry = null, CancellationToken ct = default);
}
