using System.Text.Json.Serialization;

namespace Light.Results.AspNetCore.MinimalApis.Tests;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default
)]
[JsonSerializable(typeof(Result<string>))]
[JsonSerializable(typeof(Result<ContactDto>))]
public sealed partial class ExtendedMinimalApiJsonContext : JsonSerializerContext;
