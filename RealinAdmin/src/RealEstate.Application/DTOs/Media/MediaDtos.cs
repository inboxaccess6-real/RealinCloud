namespace RealEstate.Application.DTOs.Media;

public record MediaResponse(
    Guid Id,
    Guid PropertyId,
    string Url,
    string? ContentType,
    DateTime UploadedAt
);
