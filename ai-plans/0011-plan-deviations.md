# 0011 Plan Deviations

This document compares the original plan in `ai-plans/0011-http-response-deserialization.md` with what was implemented on branch `11-http-response-message-integration` compared to `main`.

## Deviations From Original Plan

1. `ReadResultAsync` API shape changed.
- Planned:
  - `ReadResultAsync(..., LightResultHttpReadOptions? options = null, JsonSerializerOptions? serializerOptions = null, ...)`
- Implemented:
  - `ReadResultAsync(..., LightResultsHttpReadOptions? options = null, ...)`
  - Serializer options are configured via `LightResultsHttpReadOptions.SerializerOptions`.

2. Converter strategy changed to explicit read/write separation.
- Planned:
  - Implement `Read` in `DefaultResultJsonConverter`, `DefaultResultJsonConverter<T>`, `MetadataObjectJsonConverter`, `MetadataValueJsonConverter`.
- Implemented:
  - Dedicated read converters (`HttpRead*`) and write converters (`HttpWrite*`).
  - Write converters throw in `Read`.
  - Read converters throw in `Write`.

3. Namespace and folder structure is more granular than planned.
- Planned:
  - Centralized/shared serializer helper area.
- Implemented:
  - `Light.Results.Http.Reading`, `Light.Results.Http.Reading.Headers`, `Light.Results.Http.Reading.Json`
  - `Light.Results.Http.Writing`, `Light.Results.Http.Writing.Headers`, `Light.Results.Http.Writing.Json`
  - Legacy mixed serializer/header namespaces were removed.

4. Header parsing default behavior changed.
- Planned (initial draft):
  - Header parsing behavior implied by options, with default behavior effectively oriented to parsing headers.
  - Default primitive parsing proposed via JSON parse fallback.
- Implemented:
  - `HeaderSelectionMode` default is `None` (no header parsing unless opted in).
  - Default parsing is primitive-first (`bool` -> `Int64` -> `double` -> `string`), no JSON parsing of header text.

5. Read serializer defaults were adjusted for HTTP JSON conventions.
- Planned:
  - No explicit requirement for serializer defaults preset.
- Implemented:
  - `HttpReadJsonSerializerOptionsCache` uses `JsonSerializerDefaults.Web` to align with Minimal API payload casing conventions in round-trip scenarios.

6. Option type naming differs.
- Planned:
  - `LightResultHttpReadOptions`.
- Implemented:
  - `LightResultsHttpReadOptions`.

## Missing Parts To Tackle In This Branch

1. Add explicit tests for `JsonSerializerContext` usage with `ReadResultAsync`.
- Validate that read deserialization works correctly when callers provide source-generated serializer metadata.
- Suggested scenarios:
  - `ReadResultAsync<T>` success payload (bare and wrapped) with `SerializerOptions` backed by a test `JsonSerializerContext`.
  - `ReadResultAsync<T>` failure/problem-details payload with source-generated metadata for involved DTOs.
  - `ReadResultAsync` (non-generic) success/failure payloads with context-backed options.

2. Keep the current converter factory approach unless tests disprove it.
- `HttpWriteResultJsonConverterFactory` remains as implemented.
- The missing action item is verification via tests, not redesign of the factory.

3. Migration notes are intentionally out of scope for this branch.
- Library is not released yet, so no migration-doc work is required now.
