using System;
using Light.Results.Metadata;

namespace Light.Results;

/// <summary>
/// Represents an error with a message, optional code, target, metadata, source, correlation ID, and category.
/// </summary>
public readonly record struct Error(
    string Message,
    string? Code = null,
    string? Target = null,
    MetadataObject? Metadata = null,
    string? Source = null,
    Guid? CorrelationId = null,
    ErrorCategory Category = ErrorCategory.Unclassified
)
{
    /// <summary>
    /// Creates a new <see cref="Error" /> with additional metadata properties.
    /// </summary>
    public Error WithMetadata(params (string Key, MetadataValue Value)[] properties)
    {
        var newMetadata = Metadata?.With(properties) ?? MetadataObject.Create(properties);
        return this with { Metadata = newMetadata };
    }

    /// <summary>
    /// Creates a new <see cref="Error" /> with the specified metadata, replacing any existing metadata.
    /// </summary>
    public Error WithMetadata(MetadataObject metadata) => this with { Metadata = metadata };

    /// <summary>
    /// Creates a new <see cref="Error" /> with the specified source.
    /// </summary>
    public Error WithSource(string source) => this with { Source = source };

    /// <summary>
    /// Creates a new <see cref="Error" /> with the specified correlation ID.
    /// </summary>
    public Error WithCorrelationId(Guid correlationId) => this with { CorrelationId = correlationId };

    /// <summary>
    /// Creates a new <see cref="Error" /> with the specified category.
    /// </summary>
    public Error WithCategory(ErrorCategory category) => this with { Category = category };

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static Error Validation(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Validation);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static Error NotFound(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.NotFound);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static Error Conflict(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Conflict);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Unauthorized);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Forbidden);

    /// <summary>
    /// Creates a dependency failure error.
    /// </summary>
    public static Error DependencyFailure(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.DependencyFailure);

    /// <summary>
    /// Creates a transient error.
    /// </summary>
    public static Error Transient(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Transient);

    /// <summary>
    /// Creates a rate limited error.
    /// </summary>
    public static Error RateLimited(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.RateLimited);

    /// <summary>
    /// Creates an unexpected error.
    /// </summary>
    public static Error Unexpected(
        string message,
        string? code = null,
        string? target = null,
        MetadataObject? metadata = null,
        string? source = null,
        Guid? correlationId = null
    ) => new (message, code, target, metadata, source, correlationId, ErrorCategory.Unexpected);
}
