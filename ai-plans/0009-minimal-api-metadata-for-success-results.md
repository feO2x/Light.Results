# Metadata Serialization for Success Results in Minimal APIs

## Goals & Constraints

1. **Opt-in feature**: Allow callers to include `MetadataObject` in success responses, changing the response schema.
2. **Per-value control**: Enable fine-grained control over where each metadata value is serialized (HTTP body, headers, or both).
3. **Performance**: Maintain the low-allocation design of Light.Results; use source-generated JSON serialization.
4. **Schema clarity**: When metadata is included, the response body uses a wrapped format: `{ "value": T, "metadata": {...} }`.
5. **Native AOT compatibility**: All new types must work with `<IsAotCompatible>true</IsAotCompatible>`.
6. **Minimal API focus**: This plan covers ASP.NET Core Minimal APIs only.

## Current State Analysis

### Success Response Handling (Current)

From `MinimalApiResultExtensions.cs`:

```csharp
if (result.IsValid)
{
    return TypedResults.Ok(result.Value);  // Metadata is discarded
}
```

- `Result<T>.Metadata` is only serialized when the result has errors (via `LightProblemDetailsResult`).
- Success responses return `T` directly, losing any attached metadata.

### Use Cases for Metadata on Success

| Use Case | Example Metadata |
|----------|------------------|
| Pagination | `totalCount`, `nextPageToken`, `hasMore` |
| Cache hints | `etag`, `lastModified` |
| Audit/tracing | `correlationId`, `processingTimeMs` |
| Warnings | Non-fatal issues that didn't prevent success |
| Feature flags | `experimentGroup`, `featureEnabled` |

## Design Decisions

### 1. Opt-In Enum for Metadata Serialization

**Decision**: Introduce `MetadataSerializationMode` enum to control when metadata is serialized.

```csharp
/// <summary>
/// Specifies when metadata should be serialized in HTTP responses.
/// </summary>
public enum MetadataSerializationMode
{
    /// <summary>
    /// Metadata is only serialized for error responses (Problem Details).
    /// Success responses return the value directly without metadata.
    /// This is the default behavior.
    /// </summary>
    ErrorsOnly,

    /// <summary>
    /// Metadata is always serialized, even for success responses.
    /// Success responses use a wrapped format: { "value": T, "metadata": {...} }.
    /// </summary>
    Always
}
```

**Rationale**:
- `ErrorsOnly` preserves backward compatibility and clean API contracts.
- `Always` enables rich metadata transport for specialized use cases.

### 2. Wrapped Response Format

When `MetadataSerializationMode.Always` is used, success responses use this structure:

```json
{
  "value": { /* T */ },
  "metadata": {
    "totalCount": 42,
    "correlationId": "abc-123"
  }
}
```

**Design notes**:
- `value` contains the serialized `T`.
- `metadata` is omitted if `MetadataObject` is `null` (to avoid `"metadata": null`), but included as `{}` if empty to maintain schema consistency.
- For `Result` (void), the response is `{ "metadata": {...} }` with HTTP 200 (not 204, since there's a body).

### 3. Custom IResult Implementation

**Decision**: Create `LightSuccessResult<T>` implementing Minimal API's `IResult` for wrapped success responses.

**Rationale**:
- Mirrors the performance approach of `LightProblemDetailsResult`.
- Enables source-generated JSON serialization.
- Implements `IResultTypeMetadata<T>` for OpenAPI schema generation.

```csharp
/// <summary>
/// Success response that includes metadata in a wrapped format.
/// Implements IResult for direct HTTP response writing.
/// </summary>
public sealed class LightSuccessResult<T> : IResult, IResultTypeMetadata<LightSuccessResult<T>>
{
    public T Value { get; }
    public MetadataObject? Metadata { get; }
    public int StatusCode { get; }
    public JsonSerializerOptions SerializerOptions { get; }

    public Task ExecuteAsync(HttpContext httpContext);
}
```

### 4. Non-Generic Void Result

For `Result` (void) with metadata, create `LightSuccessResult` (non-generic):

```csharp
/// <summary>
/// Success response for void results that includes metadata.
/// </summary>
public sealed class LightSuccessResult : IResult, IResultTypeMetadata<LightSuccessResult>
{
    public MetadataObject Metadata { get; }
    public int StatusCode { get; }
    public JsonSerializerOptions SerializerOptions { get; }

    public Task ExecuteAsync(HttpContext httpContext);
}
```

**Response format**:
```json
{
  "metadata": {
    "correlationId": "abc-123"
  }
}
```

### 5. Extension Method Modifications (Breaking Change)

Modify the existing `ToMinimalApiResult` methods to include `MetadataSerializationMode` as a new parameter with a default value of `ErrorsOnly`. This is a breaking change for callers using named arguments, but since the library isn't published yet, this is acceptable.

```csharp
public static IResult ToMinimalApiResult<T>(
    this Result<T> result,
    HttpContext? httpContext = null,
    bool firstCategoryIsLeadingCategory = false,
    string? instance = null,
    ErrorSerializationFormat errorFormat = ErrorSerializationFormat.AspNetCoreCompatible,
    MetadataSerializationMode metadataMode = MetadataSerializationMode.ErrorsOnly);

public static IResult ToMinimalApiResult<T>(
    this Result<T> result,
    Func<Result<T>, IResult> onSuccess,
    HttpContext? httpContext = null,
    bool firstCategoryIsLeadingCategory = false,
    string? instance = null,
    ErrorSerializationFormat errorFormat = ErrorSerializationFormat.AspNetCoreCompatible,
    MetadataSerializationMode metadataMode = MetadataSerializationMode.ErrorsOnly);

public static IResult ToMinimalApiResult(
    this Result result,
    HttpContext? httpContext = null,
    bool firstCategoryIsLeadingCategory = false,
    string? instance = null,
    ErrorSerializationFormat errorFormat = ErrorSerializationFormat.AspNetCoreCompatible,
    MetadataSerializationMode metadataMode = MetadataSerializationMode.ErrorsOnly);

public static IResult ToMinimalApiResult(
    this Result result,
    Func<Result, IResult> onSuccess,
    HttpContext? httpContext = null,
    bool firstCategoryIsLeadingCategory = false,
    string? instance = null,
    ErrorSerializationFormat errorFormat = ErrorSerializationFormat.AspNetCoreCompatible,
    MetadataSerializationMode metadataMode = MetadataSerializationMode.ErrorsOnly);
```

**Rationale for parameter order**: `metadataMode` is placed last because:
- It's the newest parameter and least commonly used.
- Default value of `ErrorsOnly` preserves existing behavior.
- Callers opting in will typically use named arguments: `metadataMode: MetadataSerializationMode.Always`.

### 6. Behavior Matrix

| Result State | MetadataSerializationMode | Response |
|--------------|---------------------------|----------|
| Success | `ErrorsOnly` | `TypedResults.Ok(value)` — metadata discarded |
| Success | `Always` | `LightSuccessResult<T>` — wrapped format (always) |
| Error | Any | `LightProblemDetailsResult` — metadata in extensions |

**Schema consistency**: When `MetadataSerializationMode.Always` is specified, we **always** use `LightSuccessResult<T>` regardless of whether metadata is present. This ensures a consistent response schema for API consumers:

```json
{
  "value": { /* T */ },
  "metadata": { /* empty {} if no metadata, omitted only if null */ }
}
```

**Rationale**:
- Predictable response shape enables typed client generation (OpenAPI/Swagger).
- Simplifies client-side parsing—no conditional logic needed.
- Follows the principle of least surprise.
- Minor payload overhead (empty `"metadata": {}`) is acceptable for schema consistency.

### 7. JSON Serialization Strategy

Two converters are needed:

#### LightSuccessResultJsonConverter (Non-Generic)

For `LightSuccessResult` (corresponding to `Result`):
- Writes `{ "metadata": {...} }` structure.
- Uses the existing `MetadataObjectJsonConverter` for metadata serialization.

```csharp
public class LightSuccessResultJsonConverter : JsonConverter<LightSuccessResult>
{
    public override void Write(Utf8JsonWriter writer, LightSuccessResult value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("metadata");
        // Serialize MetadataObject using existing converter
        writer.WriteEndObject();
    }
}
```

#### LightSuccessResultJsonConverter<T> (Generic)

For `LightSuccessResult<T>` (corresponding to `Result<T>`):
1. Writes `{ "value": ... }` using the serializer for `T`.
2. Writes `"metadata": ...` using the existing `MetadataObjectJsonConverter`.

**Challenge**: Generic type `T` requires runtime type resolution for source-generated serialization.

**Solution**: Accept `JsonSerializerOptions` in the constructor (like `LightProblemDetailsResult`), allowing callers to provide options with their own `JsonSerializerContext` that includes `T`.

```csharp
public LightSuccessResult(
    T value,
    MetadataObject? metadata,
    int statusCode = 200,
    JsonSerializerOptions? serializerOptions = null)
```

### 8. OpenAPI Schema Generation

#### Complexity

There are **two sources of complexity** for OpenAPI schema generation:

1. **Generic type `T`**: The `value` property's schema depends on `T`, which is only known at runtime. ASP.NET Core's `IEndpointMetadataProvider` uses static methods, making it difficult to provide type-specific schema information.

2. **Dynamic metadata structure**: `MetadataObject` is a flexible key-value store—its schema varies per endpoint. OpenAPI expects a fixed schema, but metadata keys/types are determined by the application logic, not the type system.

Other approaches were considered (e.g., requiring a metadata type parameter on every call, or only supporting untyped schemas), but a **hybrid approach** provides the best balance of simplicity and extensibility.

#### Approach: Hybrid Schema Generation

1. **Default behavior**: `LightSuccessResult<T>` implements `IEndpointMetadataProvider` and generates a schema with `metadata` as a generic object (`additionalProperties: true`). This requires no configuration.

2. **Opt-in typed metadata**: Users who want fully typed OpenAPI schemas can use the `ProducesLightResult<TValue, TMetadata>()` extension method to register a specific metadata type.

#### Usage

```csharp
// Zero-config: metadata is "object" with additionalProperties in OpenAPI
app.MapGet("/items", GetItems);

// Typed metadata: full schema in OpenAPI
app.MapGet("/items", GetItems)
   .ProducesLightResult<List<Item>, PaginationMetadata>();
```

#### Default Schema (Zero-Config)

When no metadata type is specified, the generated OpenAPI schema uses `additionalProperties`:

```json
{
  "type": "object",
  "properties": {
    "value": { "$ref": "#/components/schemas/ItemList" },
    "metadata": {
      "type": "object",
      "additionalProperties": true
    }
  }
}
```

#### Typed Schema (Opt-In)

When using `ProducesLightResult<TValue, TMetadata>()`, the schema includes the full metadata type:

```json
{
  "type": "object",
  "properties": {
    "value": { "$ref": "#/components/schemas/ItemList" },
    "metadata": { "$ref": "#/components/schemas/PaginationMetadata" }
  }
}
```

#### Implementation

```csharp
public static class LightResultEndpointExtensions
{
    /// <summary>
    /// Adds OpenAPI response metadata for LightSuccessResult with typed metadata schema.
    /// </summary>
    public static RouteHandlerBuilder ProducesLightResult<TValue, TMetadata>(
        this RouteHandlerBuilder builder,
        int statusCode = 200,
        string? contentType = "application/json")
    {
        return builder.Produces<WrappedResponse<TValue, TMetadata>>(statusCode, contentType);
    }
}

/// <summary>
/// Schema-only type for OpenAPI documentation. Not used at runtime.
/// </summary>
public sealed class WrappedResponse<TValue, TMetadata>
{
    public TValue Value { get; init; } = default!;
    public TMetadata Metadata { get; init; } = default!;
}
```

#### User-Defined Metadata Types

Users define their metadata shape as a regular C# type:

```csharp
// User defines their metadata shape
public record PaginationMetadata(int TotalCount, string? NextPageToken, bool HasMore);

// Endpoint with typed metadata schema
app.MapGet("/items", async () =>
{
    var result = await GetItemsAsync();
    return result
        .WithMetadata("totalCount", 42)
        .WithMetadata("nextPageToken", "abc123")
        .WithMetadata("hasMore", true)
        .ToMinimalApiResult(metadataMode: MetadataSerializationMode.Always);
})
.ProducesLightResult<List<Item>, PaginationMetadata>();
```

**Note**: The `PaginationMetadata` type is only used for OpenAPI schema generation. At runtime, `MetadataObject` is still used for flexibility. Users are responsible for ensuring the runtime metadata matches the documented schema.

### 9. Per-Value Serialization Annotations

In addition to controlling metadata serialization at the result level, we provide **per-value control** by adding annotations to `MetadataValue`. This enables fine-grained decisions about where each metadata value should be serialized (HTTP response body, HTTP headers, or both).

#### MetadataValueAnnotation Flags Enum

Add a new flags enum to the core `Light.Results` project:

```csharp
namespace Light.Results.Metadata;

/// <summary>
/// Specifies where a metadata value should be serialized in HTTP responses.
/// </summary>
[Flags]
public enum MetadataValueAnnotation
{
    /// <summary>
    /// No annotation specified. Serialization behavior is determined by the serialization mode.
    /// </summary>
    None = 0,

    /// <summary>
    /// Serialize this value in the HTTP response body.
    /// </summary>
    SerializeInHttpResponseBody = 1,

    /// <summary>
    /// Serialize this value as an HTTP response header.
    /// Only valid for primitive types and arrays of primitives.
    /// </summary>
    SerializeInHttpHeader = 2,

    /// <summary>
    /// Serialize this value in both the HTTP response body and as a header.
    /// </summary>
    SerializeInBoth = SerializeInHttpResponseBody | SerializeInHttpHeader
}
```

#### MetadataValue Changes

Add an `Annotation` property to `MetadataValue`:

```csharp
public readonly struct MetadataValue : IEquatable<MetadataValue>
{
    private readonly MetadataPayload _payload;

    private MetadataValue(MetadataKind kind, MetadataPayload payload, MetadataValueAnnotation annotation)
    {
        Kind = kind;
        _payload = payload;
        Annotation = annotation;
    }

    public MetadataKind Kind { get; }
    public MetadataValueAnnotation Annotation { get; }  // NEW

    // Factory methods updated to accept optional annotation
    public static MetadataValue FromString(string? value, MetadataValueAnnotation annotation = MetadataValueAnnotation.None) =>
        value is null ? Null : new MetadataValue(MetadataKind.String, new MetadataPayload(value), annotation);

    // ... similar updates for other factory methods
}
```

#### Header Serialization Constraints

`SerializeInHttpHeader` is only valid for types that can be represented as HTTP header values:

| MetadataKind | Header-Compatible | Notes |
|--------------|-------------------|-------|
| `Null` | No | Omit the header entirely |
| `Boolean` | Yes | `"true"` or `"false"` |
| `Int64` | Yes | String representation |
| `Double` | Yes | String representation |
| `String` | Yes | Direct value |
| `Array` | Conditional | Only if all elements are primitives |
| `Object` | No | Too complex for headers |

**Validation**: Factory methods throw `ArgumentException` if `SerializeInHttpHeader` is set on an incompatible type.

#### Array Header Serialization

For arrays of primitives, use **comma-separated values** (per RFC 7230):

```
X-Tags: foo, bar, baz
```

Alternatively, multiple headers with the same name could be used, but comma-separation is more compact and widely supported.

#### Usage Example

```csharp
var result = Result<string[]>
    .Ok(items)
    .MergeMetadata(
        ("correlationId", MetadataValue.FromString(correlationId, MetadataValueAnnotation.SerializeInHttpHeader)),
        ("totalCount", MetadataValue.FromInt64(42, MetadataValueAnnotation.SerializeInHttpResponseBody)),
        ("etag", MetadataValue.FromString(etag, MetadataValueAnnotation.SerializeInBoth))
    );
```

This would produce:
- **Headers**: `X-CorrelationId: abc-123`, `ETag: "xyz"`
- **Body**: `{ "value": [...], "metadata": { "totalCount": 42, "etag": "xyz" } }`

#### Header Naming Convention

Metadata keys are converted to HTTP header names using a configurable strategy:
- Default: `X-{PascalCase(key)}` (e.g., `correlationId` → `X-CorrelationId`)
- Well-known headers: `etag` → `ETag`, `lastModified` → `Last-Modified`

#### Impact on Struct Size

Adding `MetadataValueAnnotation` (4 bytes) to `MetadataValue` is acceptable given the flexibility it provides. The struct remains small and stack-allocated.

## API Design

### MetadataSerializationMode Enum

```csharp
namespace Light.Results.AspNetCore.Shared;

/// <summary>
/// Specifies when metadata should be serialized in HTTP responses.
/// </summary>
public enum MetadataSerializationMode
{
    /// <summary>
    /// Metadata is only serialized for error responses (Problem Details).
    /// Success responses return the value directly without metadata.
    /// This is the default behavior.
    /// </summary>
    ErrorsOnly,

    /// <summary>
    /// Metadata is always serialized, even for success responses.
    /// Success responses use a wrapped format: { "value": T, "metadata": {...} }.
    /// </summary>
    Always
}
```

### LightSuccessResult<T> Class

```csharp
namespace Light.Results.AspNetCore.MinimalApis;

/// <summary>
/// Success response that includes metadata in a wrapped format.
/// Response body: { "value": T, "metadata": {...} }
/// </summary>
[JsonConverter(typeof(LightSuccessResultJsonConverter<>))]
public sealed class LightSuccessResult<T> : IResult, IResultTypeMetadata<LightSuccessResult<T>>
{
    public LightSuccessResult(
        T value,
        MetadataObject? metadata,
        int statusCode = 200,
        JsonSerializerOptions? serializerOptions = null);

    /// <summary>The result value.</summary>
    public T Value { get; }

    /// <summary>Result-level metadata.</summary>
    public MetadataObject? Metadata { get; }

    /// <summary>HTTP status code (default 200).</summary>
    public int StatusCode { get; }

    /// <summary>Serializer options for JSON output.</summary>
    public JsonSerializerOptions SerializerOptions { get; }

    public Task ExecuteAsync(HttpContext httpContext);

    // Not all members are shown for brevity
}
```

### LightSuccessResult Class (Non-Generic)

```csharp
namespace Light.Results.AspNetCore.MinimalApis;

/// <summary>
/// Success response for void results that includes metadata.
/// Response body: { "metadata": {...} }
/// </summary>
[JsonConverter(typeof(LightSuccessResultJsonConverter))]
public sealed class LightSuccessResult : IResult, IResultTypeMetadata<LightSuccessResult>
{
    public LightSuccessResult(
        MetadataObject metadata,
        int statusCode = 200,
        JsonSerializerOptions? serializerOptions = null);

    /// <summary>Result-level metadata.</summary>
    public MetadataObject Metadata { get; }

    /// <summary>HTTP status code (default 200).</summary>
    public int StatusCode { get; }

    /// <summary>Serializer options for JSON output.</summary>
    public JsonSerializerOptions SerializerOptions { get; }

    public Task ExecuteAsync(HttpContext httpContext);

    // Not all members are shown for brevity
}
```

### IHeaderNamingStrategy Interface

```csharp
namespace Light.Results.AspNetCore.Shared;

/// <summary>
/// Strategy for converting metadata keys to HTTP header names.
/// </summary>
public interface IHeaderNamingStrategy
{
    /// <summary>
    /// Converts a metadata key to an HTTP header name.
    /// </summary>
    /// <param name="key">The metadata key (e.g., "correlationId").</param>
    /// <returns>The HTTP header name (e.g., "X-CorrelationId").</returns>
    string ToHeaderName(string key);
}

/// <summary>
/// Default header naming strategy: converts camelCase keys to X-PascalCase headers,
/// with special handling for well-known headers.
/// </summary>
public sealed class DefaultHeaderNamingStrategy : IHeaderNamingStrategy
{
    public static DefaultHeaderNamingStrategy Instance { get; } = new();

    public string ToHeaderName(string key)
    {
        // Well-known headers
        return key.ToLowerInvariant() switch
        {
            "etag" => "ETag",
            "lastmodified" => "Last-Modified",
            "contenttype" => "Content-Type",
            "cachecontrol" => "Cache-Control",
            _ => $"X-{ToPascalCase(key)}"
        };
    }

    private static string ToPascalCase(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;

        // Implement an efficient span-based conversion here, where
        // `char.ToUpperInvariant` is used on the first character
        return ...;
    }
}
```

### Annotation Behavior with MetadataSerializationMode

The `MetadataValueAnnotation` flags interact with `MetadataSerializationMode` as follows:

| MetadataSerializationMode | Annotation | Body Serialization | Header Serialization |
|---------------------------|------------|-------------------|---------------------|
| `ErrorsOnly` | `None` | No | No |
| `ErrorsOnly` | `SerializeInHttpResponseBody` | No | No |
| `ErrorsOnly` | `SerializeInHttpHeader` | No | **Yes** |
| `ErrorsOnly` | `SerializeInBoth` | No | **Yes** |
| `Always` | `None` | Yes | No |
| `Always` | `SerializeInHttpResponseBody` | Yes | No |
| `Always` | `SerializeInHttpHeader` | No | Yes |
| `Always` | `SerializeInBoth` | Yes | Yes |

**Key insight**: Header annotations are honored regardless of `MetadataSerializationMode`. This allows metadata like `correlationId` to be written to headers even when the body uses the unwrapped format (`ErrorsOnly`).

## Project Structure Changes

```
src/Light.Results/
├── Metadata/
│   ├── MetadataValueAnnotation.cs            # NEW: Flags enum for serialization targets
│   └── MetadataValue.cs                      # MODIFIED: Add Annotation property

src/Light.Results.AspNetCore.Shared/
├── MetadataSerializationMode.cs              # NEW: Enum for opt-in behavior
├── IHeaderNamingStrategy.cs                  # NEW: Interface for header name conversion
└── DefaultHeaderNamingStrategy.cs            # NEW: Default implementation (X-PascalCase)

src/Light.Results.AspNetCore.MinimalApis/
├── LightSuccessResult.cs                     # NEW: Non-generic void result with metadata
├── LightSuccessResult{T}.cs                  # NEW: Generic result with metadata
├── LightResultEndpointExtensions.cs          # NEW: ProducesLightResult extension method
├── WrappedResponse{TValue,TMetadata}.cs      # NEW: Schema-only type for OpenAPI
├── Serialization/
│   ├── LightSuccessResultJsonConverter.cs    # NEW: Converter for non-generic
│   └── LightSuccessResultJsonConverter{T}.cs # NEW: Converter for generic
└── MinimalApiResultExtensions.cs             # MODIFIED: Add metadataMode parameter, header serialization

tests/Light.Results.Tests/
├── MetadataValueAnnotationTests.cs           # NEW: Annotation validation tests

tests/Light.Results.AspNetCore.MinimalApis.Tests/
├── LightSuccessResultTests.cs                # NEW
├── LightSuccessResultGenericTests.cs         # NEW
├── MetadataSerializationModeTests.cs         # NEW
└── HeaderSerializationTests.cs               # NEW: Per-value header serialization tests
```

## Implementation Steps

### Phase 1: MetadataSerializationMode Enum

1. Create `MetadataSerializationMode.cs` in `Light.Results.AspNetCore.Shared`.
2. Document the enum values clearly.

### Phase 2: LightSuccessResult<T>

1. Create `LightSuccessResult{T}.cs`:
   - Constructor accepting `T value`, `MetadataObject? metadata`, `int statusCode`, `JsonSerializerOptions?`.
   - Implement `IResult.ExecuteAsync()` to write wrapped JSON.
2. Create `LightSuccessResultJsonConverter{T}.cs`:
   - Write `{ "value": ..., "metadata": ... }` structure.
   - Omit `metadata` property only if `null`; include as `{}` if empty for schema consistency.

### Phase 3: LightSuccessResult (Non-Generic)

1. Create `LightSuccessResult.cs`:
   - Constructor accepting `MetadataObject metadata`, `int statusCode`, `JsonSerializerOptions?`.
   - Throw if metadata is empty (no point in wrapping without metadata).
2. Create `LightSuccessResultJsonConverter.cs`:
   - Write `{ "metadata": ... }` structure.

### Phase 4: MetadataValueAnnotation (Core Library)

1. Create `MetadataValueAnnotation.cs` in `Light.Results/Metadata/`:
   - Flags enum with `None`, `SerializeInHttpResponseBody`, `SerializeInHttpHeader`, `SerializeInBoth`.
2. Modify `MetadataValue.cs`:
   - Add `Annotation` property.
   - Update constructor to accept annotation parameter.
   - Update all factory methods (`FromBoolean`, `FromInt64`, `FromDouble`, `FromString`, `FromArray`, `FromObject`) to accept optional annotation with default `None`.
   - Add validation: throw `ArgumentException` if `SerializeInHttpHeader` is set on `Object` or `Array` with non-primitive elements.
3. Update implicit conversions to use `None` annotation.
4. Write `MetadataValueAnnotationTests` in `Light.Results.Tests`.

### Phase 5: Extension Method Modifications

1. Modify existing `ToMinimalApiResult` methods in `MinimalApiResultExtensions` to add `metadataMode` parameter.
2. Implement behavior matrix logic:
   - `ErrorsOnly` → existing behavior (return value directly).
   - `Always` → use `LightSuccessResult<T>` or `LightSuccessResult`.
3. Add header serialization logic:
   - Iterate metadata values and check `Annotation` flags.
   - Write values with `SerializeInHttpHeader` flag to response headers.
   - Write values with `SerializeInHttpResponseBody` flag to response body.

### Phase 6: Header Naming Strategy

1. Create `IHeaderNamingStrategy` interface in `Light.Results.AspNetCore.Shared`.
2. Create `DefaultHeaderNamingStrategy`:
   - Convert camelCase keys to `X-PascalCase` headers.
   - Map well-known keys (`etag` → `ETag`, `lastModified` → `Last-Modified`).
3. Allow strategy injection via `JsonSerializerOptions` or constructor parameter.

### Phase 7: Update JsonSerializerContext

1. Add `LightSuccessResult` and `LightSuccessResult<T>` to `LightResultsMinimalApiJsonContext`.
2. Ensure converters are registered in `JsonDefaults.Options`.

### Phase 8: Tests

1. **MetadataValueAnnotationTests**: Verify annotation validation (reject header annotation on Object/incompatible Array).
2. **LightSuccessResultTests**: Verify non-generic serialization.
3. **LightSuccessResultGenericTests**: Verify generic serialization with various `T` types.
4. **MetadataSerializationModeTests**: Verify behavior matrix in extension methods.
5. **HeaderSerializationTests**: Verify per-value header serialization, comma-separated arrays, naming conventions.
6. **Integration tests**: End-to-end with `HttpContext` mocks.
