using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Light.Results.Http.Reading.Json;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// JSON converter for reading <see cref="CloudEventBareSuccessPayload{T}" /> from CloudEvents data payloads.
/// </summary>
/// <typeparam name="T">The payload value type.</typeparam>
public sealed class CloudEventBareSuccessPayloadJsonConverter<T> : JsonConverter<CloudEventBareSuccessPayload<T>>
{
    /// <summary>
    /// Reads the JSON representation of a <see cref="CloudEventBareSuccessPayload{T}" />.
    /// </summary>
    public override CloudEventBareSuccessPayload<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var httpPayload = ResultJsonReader.ReadBareSuccessPayload<T>(ref reader, options);
        return new CloudEventBareSuccessPayload<T>(httpPayload.Value);
    }

    /// <summary>
    /// Writing is not supported by this converter.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        CloudEventBareSuccessPayload<T> value,
        JsonSerializerOptions options
    ) =>
        throw new NotSupportedException(
            $"{nameof(CloudEventBareSuccessPayloadJsonConverter<T>)} supports deserialization only. Use a serialization converter for writing."
        );
}
