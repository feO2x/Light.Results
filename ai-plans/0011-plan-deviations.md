# 0011 Plan Deviations

This document compares `ai-plans/0011-http-response-deserialization.md` with the current implementation state on branch
`11-http-response-message-integration`.

## Deviations From Original Plan

1. `ReadResultAsync` API shape was simplified.
- Planned:
  - `ReadResultAsync(..., LightResultHttpReadOptions? options = null, JsonSerializerOptions? serializerOptions = null, ...)`
- Implemented:
  - `ReadResultAsync(..., LightResultsHttpReadOptions? options = null, ...)`
  - Serializer options are provided through `LightResultsHttpReadOptions.SerializerOptions`.

2. Header selection moved from mode/list options to Strategy Pattern.

- Planned:
    - `HeaderSelectionMode` + `HeaderAllowList` + `HeaderDenyList`.
- Implemented:
    - `IHttpHeaderSelectionStrategy` with built-ins in `HttpHeaderSelectionStrategies`.
    - `LightResultsHttpReadOptions.HeaderSelectionStrategy` is the single selection entry point.

3. Header parsing defaults changed to match HTTP round-trip expectations.
- Planned:
    - Default parsing proposed JSON-first fallback behavior for header values.
- Implemented:
    - Default header parsing is primitive-first (`bool` -> `Int64` -> finite `double` -> `string`).
    - Header parsing is opt-in by default because `HeaderSelectionStrategy` defaults to `None`.

4. Converters were split into explicit read/write converter sets.
- Planned:
    - Add `Read` implementations to existing converter types.
- Implemented:
    - `HttpRead*` converters for deserialization and `HttpWrite*` converters for serialization.
    - Read-only converters throw in `Write`; write-only converters throw in `Read`.
    - `DefaultResultJsonConverter` naming was replaced by `HttpWriteResultJsonConverter` family.

5. HTTP namespace structure became more explicit than originally planned.

- Planned:
    - Shared serializer-oriented grouping.
- Implemented:
  - `Light.Results.Http.Reading`, `Light.Results.Http.Reading.Headers`, `Light.Results.Http.Reading.Json`
  - `Light.Results.Http.Writing`, `Light.Results.Http.Writing.Headers`, `Light.Results.Http.Writing.Json`
  - Options and behavior enums are located in `Reading` or `Writing` according to direction.

6. `HttpResponseMessage` read path received additional allocation-focused optimization.

- Planned:
    - General focus on low allocations.
- Implemented:
    - `Content-Length` is checked first to avoid unnecessary `byte[]` allocations.
    - Known-length non-empty payloads are deserialized from stream first.
    - Fallback to byte-array path is used only when length is unknown.

7. Read serializer defaults were explicitly aligned to web JSON behavior.
- Planned:
    - No strict default serializer preset requirement.
- Implemented:
    - `HttpReadJsonSerializerOptionsCache` uses `JsonSerializerDefaults.Web` and pre-registers read converters.

8. Native AOT guidance was partially relaxed by design decision.
- Planned:
    - Avoid `Activator.CreateInstance` / `MakeGenericType` in new code paths.
- Implemented:
    - Converter factories still use runtime generic creation (`Activator` + `MakeGenericType`).
    - This is an intentional tradeoff accepted for now in this branch.

## Additional Work Completed Beyond Initial Plan

1. End-to-end round-trip integration tests were added using Minimal API apps and `HttpClient`.

- `Result` and `Result<T>` assertions use Value Object style expected-result comparisons.

2. `JsonSerializerContext`-based read tests were added.

- Context-backed `ReadResultAsync` scenarios are covered for generic success, generic failure, and non-generic success.

3. Targeted coverage improvements were added for HTTP reading/writing internals.

- Additional unit tests cover header selection strategies, parser registry behavior, parsing/conversion helpers, and
  JSON reader edge paths.
