namespace RealEstate.Application.Common;

public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Error = null
);

public record ErrorResponse(
    string Error,
    string? Details = null,
    int StatusCode = 400
);
