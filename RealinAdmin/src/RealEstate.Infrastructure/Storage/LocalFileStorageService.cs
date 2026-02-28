using Microsoft.AspNetCore.Hosting;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.ContentRootPath, "uploads", "media");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<(string Bucket, string Key)> UploadAsync(
        Stream file, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";
        var filePath = Path.Combine(_basePath, key);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        return ("local", key);
    }

    public Task DeleteAsync(string bucket, string key, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);

            var dir = Path.GetDirectoryName(filePath)!;
            if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
            {
                Directory.Delete(dir);
            }
        }
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(
        string bucket, string key, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        return Task.FromResult($"/uploads/media/{key}");
    }
}
