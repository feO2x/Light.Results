using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Light.Results.Http.Reading.Json;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// JSON converter for reading <see cref="CloudEventAutoSuccessPayload{T}" /> from CloudEvent data payloads.
/// </summary>
/// <typeparam name="T">The payload value type.</typeparam>
public sealed class CloudEventAutoSuccessPayloadJsonConverter<T> : JsonConverter<CloudEventAutoSuccessPayload<T>>
{
    /// <summary>
    /// Reads the JSON representation of a <see cref="CloudEventAutoSuccessPayload{T}" />.
    /// </summary>
    public override CloudEventAutoSuccessPayload<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var httpPayload = ResultJsonReader.ReadAutoSuccessPayload<T>(ref reader, options);
        return new CloudEventAutoSuccessPayload<T>(httpPayload.Value, httpPayload.Metadata);
    }

    /// <summary>
    /// Writing is not supported by this converter.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        CloudEventAutoSuccessPayload<T> value,
        JsonSerializerOptions options
    ) =>
        throw new NotSupportedException(
            $"{nameof(CloudEventAutoSuccessPayloadJsonConverter<T>)} supports deserialization only. Use a serialization converter for writing."
        );
}
