using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Light.Results.Http.Reading.Json;

/// <summary>
/// Provides cached <see cref="JsonSerializerOptions" /> instances tuned for HTTP response reading.
/// </summary>
public static class HttpReadJsonSerializerOptionsCache
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> CustomOptionsCache =
        new ();

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
    /// Resolves serializer options for HTTP result reading.
    /// </summary>
    /// <param name="serializerOptions">
    /// Optional serializer options supplied by the caller. The returned options include all required
    /// HTTP read converters.
    /// </param>
    /// <returns>The resolved serializer options.</returns>
    public static JsonSerializerOptions GetOrCreate(JsonSerializerOptions? serializerOptions)
    {
        if (serializerOptions is null)
        {
            return Default;
        }

        return CustomOptionsCache.GetValue(
            serializerOptions,
            static options =>
            {
                var clonedOptions = new JsonSerializerOptions(options);
                EnsureReadConverters(clonedOptions);
                return clonedOptions;
            }
        );
    }

    /// <summary>
    /// Creates the default serializer options with the converters required for reading HTTP results.
    /// </summary>
    private static JsonSerializerOptions CreateDefault()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        EnsureReadConverters(serializerOptions);
        return serializerOptions;
    }

    private static void EnsureReadConverters(JsonSerializerOptions serializerOptions)
    {
        if (!ContainsConverter<HttpReadMetadataObjectJsonConverter>(serializerOptions))
        {
            serializerOptions.Converters.Add(new HttpReadMetadataObjectJsonConverter());
        }

        if (!ContainsConverter<HttpReadMetadataValueJsonConverter>(serializerOptions))
        {
            serializerOptions.Converters.Add(new HttpReadMetadataValueJsonConverter());
        }

        if (!ContainsConverter<HttpReadFailureResultPayloadJsonConverter>(serializerOptions))
        {
            serializerOptions.Converters.Add(new HttpReadFailureResultPayloadJsonConverter());
        }

        if (!ContainsConverter<HttpReadSuccessResultPayloadJsonConverter>(serializerOptions))
        {
            serializerOptions.Converters.Add(new HttpReadSuccessResultPayloadJsonConverter());
        }

        if (!ContainsConverter<HttpReadSuccessResultPayloadJsonConverterFactory>(serializerOptions))
        {
            serializerOptions.Converters.Add(new HttpReadSuccessResultPayloadJsonConverterFactory());
        }
    }

    private static bool ContainsConverter<TConverter>(JsonSerializerOptions serializerOptions)
    {
        for (var i = 0; i < serializerOptions.Converters.Count; i++)
        {
            if (serializerOptions.Converters[i] is TConverter)
            {
                return true;
            }
        }

        return false;
    }
}
