using System.Text.Json;

namespace Light.Results.Http.Reading.Json;

/// <summary>
/// Provides cached <see cref="JsonSerializerOptions" /> instances tuned for the different
/// <see cref="PreferSuccessPayload" /> strategies used when reading HTTP responses.
/// </summary>
public static class HttpReadJsonSerializerOptionsCache
{
    /// <summary>
    /// Gets the default serializer options used when reading HTTP results.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = CreateDefault();

    /// <summary>
    /// Gets serializer options that automatically decide how to interpret success payloads.
    /// </summary>
    public static JsonSerializerOptions Auto { get; } = Default;

    /// <summary>
    /// Gets serializer options configured for bare value success payloads.
    /// </summary>
    public static JsonSerializerOptions BareValue { get; } = Default;

    /// <summary>
    /// Gets serializer options configured for wrapped value success payloads.
    /// </summary>
    public static JsonSerializerOptions WrappedValue { get; } = Default;

    /// <summary>
    /// Retrieves the cached serializer options that match the given <paramref name="preference" />.
    /// </summary>
    /// <param name="preference">The preferred representation for successful HTTP payloads.</param>
    /// <returns>The cached <see cref="JsonSerializerOptions" /> for the preference.</returns>
    public static JsonSerializerOptions GetByPreference(PreferSuccessPayload preference) => Default;

    /// <summary>
    /// Creates the default serializer options with the converters required for reading HTTP results.
    /// </summary>
    private static JsonSerializerOptions CreateDefault()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        serializerOptions.Converters.Add(new HttpReadMetadataObjectJsonConverter());
        serializerOptions.Converters.Add(new HttpReadMetadataValueJsonConverter());
        serializerOptions.Converters.Add(new HttpReadResultJsonConverter());
        serializerOptions.Converters.Add(new HttpReadResultJsonConverterFactory());

        return serializerOptions;
    }
}
