namespace Light.Results;

/// <summary>
/// Categorizes errors for routing, logging, and HTTP/gRPC status mapping.
/// Backed by byte to minimize struct size.
/// </summary>
public enum ErrorCategory : byte
{
    /// <summary>Default category when no specific category applies.</summary>
    Unclassified = 0,

    /// <summary>Input validation failures (HTTP 400, gRPC InvalidArgument).</summary>
    Validation = 1,

    /// <summary>Requested resource not found (HTTP 404, gRPC NotFound).</summary>
    NotFound = 2,

    /// <summary>Resource state conflict (HTTP 409, gRPC Aborted/AlreadyExists).</summary>
    Conflict = 3,

    /// <summary>Authentication required or failed (HTTP 401, gRPC Unauthenticated).</summary>
    Unauthorized = 4,

    /// <summary>Authenticated but not permitted (HTTP 403, gRPC PermissionDenied).</summary>
    Forbidden = 5,

    /// <summary>Downstream service or dependency failure (HTTP 502/503, gRPC Unavailable).</summary>
    DependencyFailure = 6,

    /// <summary>Transient error that may succeed on retry (HTTP 503, gRPC Unavailable).</summary>
    Transient = 7,

    /// <summary>Client has exceeded allowed request rate (HTTP 429, gRPC ResourceExhausted).</summary>
    RateLimited = 8,

    /// <summary>Unexpected internal error (HTTP 500, gRPC Internal).</summary>
    Unexpected = 9
}
