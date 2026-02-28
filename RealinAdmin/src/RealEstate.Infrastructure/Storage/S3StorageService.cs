using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Storage;

public class S3StorageService : IFileStorageService
{
    private readonly string _bucketName;
    private readonly RegionEndpoint _region;

    public S3StorageService(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:BucketName"]
            ?? throw new InvalidOperationException("AWS:BucketName is required in configuration.");
        var regionName = configuration["AWS:Region"]
            ?? throw new InvalidOperationException("AWS:Region is required in configuration.");
        _region = RegionEndpoint.GetBySystemName(regionName);
    }

    public async Task<(string Bucket, string Key)> UploadAsync(
        Stream file, string fileName, string contentType, CancellationToken ct = default)
    {
        using var client = new AmazonS3Client(_region);

        var key = $"{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file,
            ContentType = contentType
        };

        await client.PutObjectAsync(request, ct);

        return (_bucketName, key);
    }

    public async Task DeleteAsync(string bucket, string key, CancellationToken ct = default)
    {
        using var client = new AmazonS3Client(_region);

        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = key
        };

        await client.DeleteObjectAsync(request, ct);
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucket, string key, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        using var client = new AmazonS3Client(_region);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1))
        };

        var url = await client.GetPreSignedURLAsync(request);

        return url;
    }
}
