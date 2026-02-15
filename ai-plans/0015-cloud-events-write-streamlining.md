# CloudEvents JSON Writing Streamlining

## Rationale

The current CloudEvents writing implementation bypasses the STJ pipeline by directly using `Utf8JsonWriter` in `CloudEventResultExtensions`. While this approach was optimized in the previous iteration (replacing `MemoryStream` with `PooledByteBufferWriter`), it has fundamental architectural limitations:

1. **No converter customization**: Callers cannot exchange the default JSON converters for `Result`, `Result<T>`, or metadata types because STJ's `JsonSerializer.Serialize()` is never invoked.

2. **Constructor injection required**: `CloudEventWriteResultJsonConverter` requires `LightResultsCloudEventWriteOptions` to be passed at construction time, complicating DI scenarios and preventing stateless converters.

3. **Monolithic logic**: Envelope construction, attribute resolution, extension attribute writing, and data serialization are tightly coupled in a single class with many private helper methods.

4. **Limited composability**: Messaging libraries that use STJ cannot easily integrate with the current design.

This plan introduces an intermediary **readonly record struct** that represents a fully-resolved CloudEvent envelope. JSON converters serialize this envelope type, delegating data serialization to separate `Result`/`Result<T>` data converters. This design enables full STJ pipeline integration, stateless converters, and clean separation of concerns.

## Acceptance Criteria

### Reuse Existing Envelope Types
- [ ] The existing `CloudEventEnvelope` and `CloudEventEnvelope<T>` types in `Light.Results.CloudEvents` namespace are reused for writing (no new types needed).
- [ ] Writing code references these shared types rather than creating duplicates.

### Envelope JSON Converters
- [ ] `CloudEventEnvelopeJsonConverter` serializes `CloudEventEnvelope` by writing envelope attributes and delegating `Data` serialization to STJ.
- [ ] `CloudEventEnvelopeJsonConverter<T>` serializes `CloudEventEnvelope<T>` similarly.
- [ ] `CloudEventEnvelopeJsonConverterFactory` creates generic envelope converters for any `CloudEventEnvelope<T>`.
- [ ] Envelope converters are **stateless** (no constructor parameters).
- [ ] Envelope converters are **write-only** (`Read` throws `NotSupportedException`). Reading uses a separate flow with `CloudEventEnvelopePayload` for byte offset tracking.

### Result Data Converters (CloudEvent Context)
- [ ] `CloudEventResultDataJsonConverter` serializes `Result` as CloudEvent data payload (metadata only for success, errors + metadata for failure).
- [ ] `CloudEventResultDataJsonConverter<T>` serializes `Result<T>` as CloudEvent data payload (value + metadata for success, errors + metadata for failure).
- [ ] `CloudEventResultDataJsonConverterFactory` creates generic data converters for any `Result<T>`.
- [ ] Data converters respect `MetadataSerializationMode` from `LightResultsCloudEventWriteOptions` (retrieved via `JsonSerializerOptions`).
- [ ] Data converters are **write-only** (`Read` throws `NotSupportedException`). Reading uses separate payload converters.

### Extension Methods
- [ ] `ToCloudEventEnvelope()` extension methods on `Result` and `Result<T>` construct the envelope struct with resolved attributes.
- [ ] `ToCloudEvent()` extension methods remain available, internally constructing the envelope and calling `JsonSerializer.Serialize()`.
- [ ] `WriteCloudEvent(Utf8JsonWriter)` methods remain for callers who need direct writer control.

### Module Registration
- [ ] `AddDefaultLightResultsCloudEventWriteJsonConverters()` registers all envelope and data converters without requiring options at registration time.
- [ ] `LightResultsCloudEventWriteOptions` can be stored in `JsonSerializerOptions.TypeInfoResolver` or as a custom property for data converters to retrieve.

### Code Hygiene
- [ ] Legacy converter classes (`CloudEventWriteResultJsonConverter`, `CloudEventWriteResultJsonConverterFactory`) are removed.
- [ ] Complex logic in `CloudEventResultExtensions` is simplified by delegating to STJ.
- [ ] Existing tests pass with no regressions.
- [ ] New tests verify envelope construction and serialization.

### Benchmarks
- [ ] Benchmarks compare the new STJ-based approach against the previous direct-write approach.
- [ ] Memory allocations are tracked to ensure no regression.

## Technical Details

### Reusing Existing Envelope Types

The `CloudEventEnvelope` and `CloudEventEnvelope<T>` types already exist in `src/Light.Results/CloudEvents/CloudEventEnvelope.cs`:

```csharp
public readonly record struct CloudEventEnvelope<T>(
    string Type,
    string Source,
    string Id,
    Result<T> Data,
    string? Subject = null,
    DateTimeOffset? Time = null,
    string? DataContentType = null,
    string? DataSchema = null,
    MetadataObject? ExtensionAttributes = null
);
```

These types are already used by the reading code (`ReadResultWithCloudEventEnvelope`) and are located in the shared `Light.Results.CloudEvents` namespace. The writing code will reuse them directly.

The `ToCloudEventEnvelope()` extension method handles:
- Resolving `Type` based on `IsValid` (success vs failure type)
- Generating `Id` if not provided
- Extracting extension attributes from result metadata
- Validating `Source` as URI-reference and `DataSchema` as absolute URI

### Converter Architecture

```
CloudEvents/Writing/Json/
├── CloudEventEnvelopeJsonConverter.cs
│   ├── CloudEventEnvelopeJsonConverter (non-generic)
│   ├── CloudEventEnvelopeJsonConverter<T>
│   └── CloudEventEnvelopeJsonConverterFactory
├── CloudEventResultDataJsonConverter.cs
│   ├── CloudEventResultDataJsonConverter (non-generic)
│   ├── CloudEventResultDataJsonConverter<T>
│   └── CloudEventResultDataJsonConverterFactory
```

#### Envelope Converter Responsibilities

The envelope converter writes:
1. `specversion` (always "1.0")
2. `type`, `source`, `id`, `time` (required attributes)
3. `subject`, `dataschema` (if present)
4. `datacontenttype` (always "application/json")
5. Extension attributes (iterating `ExtensionAttributes` MetadataObject)
6. `data` property — delegates to `JsonSerializer.Serialize(envelope.Data, options)`

Because the envelope converter doesn't know about `LightResultsCloudEventWriteOptions`, it simply writes what's in the struct. All resolution logic moves to `ToCloudEventEnvelope()`.

#### Data Converter Responsibilities

The data converter writes the `Result` or `Result<T>` as the CloudEvent data payload:

**For success:**
- Non-generic `Result`: writes `{ "metadata": { ... } }` if metadata exists with `SerializeInCloudEventData` annotation, otherwise writes `null` or empty object.
- Generic `Result<T>`: writes `{ "value": ..., "metadata": { ... } }` or just the value if no metadata.

**For failure:**
- Writes `{ "errors": [...], "metadata": { ... } }` using existing error serialization helpers.

The data converter retrieves `LightResultsCloudEventWriteOptions` from `JsonSerializerOptions` to determine `MetadataSerializationMode`. This can be done via:
- A custom `IJsonTypeInfoResolver` that stores options
- The `JsonSerializerOptions.TypeInfoResolverChain` with a marker resolver
- A static `AsyncLocal<LightResultsCloudEventWriteOptions>` scoped per serialization call

Recommendation: Use a simple extension method `GetCloudEventWriteOptions(this JsonSerializerOptions)` that retrieves options from a known location (e.g., a registered singleton converter or a custom type info resolver).

### Extension Method Flow

```csharp
public static byte[] ToCloudEvent<T>(
    this Result<T> result,
    string? successType = null,
    string? failureType = null,
    string? id = null,
    string? source = null,
    ...
    LightResultsCloudEventWriteOptions? options = null
)
{
    var resolvedOptions = options ?? LightResultsCloudEventWriteOptions.Default;
    var envelope = result.ToCloudEventEnvelope(
        successType, failureType, id, source, ...
        resolvedOptions
    );
    return JsonSerializer.SerializeToUtf8Bytes(envelope, resolvedOptions.SerializerOptions);
}
```

This approach:
1. Constructs the envelope struct (no heap allocation for the struct itself)
2. Delegates entirely to STJ for serialization
3. Allows callers to replace any converter in the chain

### Handling MetadataSerializationMode in Data Converters

Since data converters need to know `MetadataSerializationMode`, and we want stateless converters, we can:

1. **Store options reference in SerializerOptions**: Add a marker converter or use `JsonSerializerOptions.UnknownTypeHandling` with a custom resolver that stores the options.

2. **Always serialize metadata in CloudEvent data**: Simplify by always including metadata with `SerializeInCloudEventData` annotation. The mode only affects whether to include it, but for CloudEvents the annotation already controls this.

Recommendation: For CloudEvents, the `MetadataValueAnnotation.SerializeInCloudEventData` annotation is the primary filter. The `MetadataSerializationMode` can be checked once when constructing the envelope (to decide whether to include metadata at all), and the data converter simply writes whatever metadata is present in the result.

### Removing Legacy Converters

The following files can be deleted or significantly simplified:
- `CloudEventWriteResultJsonConverter.cs` — replaced by envelope + data converters
- `CloudEventWriteResultJsonConverterFactory.cs` — replaced by envelope factory

The complex private methods in `CloudEventResultExtensions` (attribute resolution, validation) remain but are invoked during envelope construction rather than during serialization.

### Performance Considerations

1. **Struct envelope**: `CloudEventEnvelope<T>` is a readonly record struct, avoiding heap allocation for the envelope itself.
2. **STJ source generators**: Callers can use `JsonSerializerContext` for AOT-friendly serialization.
3. **Single STJ call**: One `JsonSerializer.Serialize()` call instead of manual writer manipulation.
4. **Potential overhead**: STJ's converter resolution has some overhead, but this is typically negligible and enables caching.

Benchmarks should compare:
- Current `PooledByteBufferWriter` + direct write approach
- New `ToCloudEventEnvelope()` + `JsonSerializer.SerializeToUtf8Bytes()` approach

If the new approach shows regression, consider keeping the direct-write path as an optimization while still offering the STJ-integrated path for extensibility.
