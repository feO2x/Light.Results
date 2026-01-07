# Plan for Result/Errors performance & metadata enhancements

## Goals

1. Tighten `Result<T>` / `Result` hot-path performance: minimize allocations, avoid unnecessary copying, and keep struct size predictable.
2. Extend `Error`/`Errors` information model with service-level metadata (`Source`, `CorrelationId`, `Category`) without regressing performance.
3. Maintain RFC 9457/gRPC-friendly metadata surfaces introduced in plan 0001.

## Current snapshot

- `Result<T>` is a readonly struct holding `T Value`, `Errors _errors`, and `MetadataObject? _metadata`. Success path stores metadata as nullable value type but still pays an extra field.
- `Errors` uses SBO: single error inline, multiple in `Error[]`.
- `Error` currently contains message, code, target, metadata only.

## Proposed optimizations

### 1. Struct layout & field packing

- Reorder `Result<T>` fields (`Errors _errors`, `MetadataObject? _metadata`, `T Value`, `bool IsSuccess`) to reduce padding. Measure `Unsafe.SizeOf<Result<T>>()` before/after.
- Consider `MetadataObject?` → `MetadataObjectReference` (internal struct containing pointer + bool) to avoid Nullable boxing overhead.
- Provide `internal Result(T value, Errors errors, MetadataObject? metadata, bool isSuccess)` factory to avoid duplicate constructors.

### 2. Avoid redundant metadata merging/copies

- In `Bind`/`Map`, skip metadata merging if references are equal (`ReferenceEquals`). Already partially done; enforce across all helper methods.
- Introduce `MetadataObject.MergeIfNeeded(existing, incoming, strategy)` returning same reference when incoming is empty; propagate to `Result<T>.MergeMetadata`.

### 3. Errors storage, enumeration & conversions

- Remove `Result<T>.ErrorList` (`ImmutableArray<Error>`) entirely; instead expose the internal `Errors` struct via `public Errors Errors => _errors;`.
- Rework `Errors` to keep `_one` for SBO plus a `ReadOnlyMemory<Error>` for multi-error cases.
- Implement `Errors : IReadOnlyList<Error>` with a custom value-type enumerator that stores `_one`/`_many` references and iterates without a compiler-generated state machine (single case uses `_one`, multi case indexes `_many.Span`). Indexer and `Count` become zero-cost operations.

### 4. Result&lt;T> TryGetValue

- Add `Result<T>.TryGetValue(out T value)` that returns `true` for success without throwing.
- When false is returned, `Errors` is not null. Use appropriate NRT attributes to help the compiler understand this.

## Error model enhancements

### New fields

- `string Source` (nullable). Why string? Service names rarely need GUID space & strings map cleanly to RFC 9457 `instance`/`extensions`. Provide helper `Error.WithSource(string)`. Use interned strings where possible (e.g., static readonly for known services) to reduce allocations.
- `CorrelationId` with type `Guid`.
- `ErrorCategory Category` enum covering Validation, NotFound, Conflict, Unauthorized, Forbidden, DependencyFailure, Transient, Unexpected. Backed by byte to keep struct tiny; default = `ErrorCategory.Unclassified` (0).

### API & perf implications

- Update `Error` record struct signature to include new fields with default values to preserve existing call sites:
  ```csharp
  public readonly record struct Error(
      string Message,
      string? Code = null,
      string? Target = null,
      MetadataObject? Metadata = null,
      string? Source = null,
      Guid? CorrelationId = null,
      ErrorCategory Category = ErrorCategory.Unclassified)
  ```
- Ensure `Errors` SBO still works: when storing single error, new fields live inline; when many, array holds enriched errors.
- Provide factory helpers: `Error.Validation(string message, string? target = null, ...)` to set categories & defaults without repeated args.
- Update serialization adapters (RFC 9457/gRPC) to map new properties:
  - `Source` → `extensions.source`
  - `CorrelationId` → `traceId` equivalent
  - `Category` → `extensions.category`
