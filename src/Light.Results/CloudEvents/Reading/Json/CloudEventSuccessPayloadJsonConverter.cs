using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Light.Results.Http.Reading.Json;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// JSON converter for reading <see cref="CloudEventSuccessPayload" /> from CloudEvents data payloads.
/// </summary>
public sealed class CloudEventSuccessPayloadJsonConverter : JsonConverter<CloudEventSuccessPayload>
{
    /// <summary>
    /// Reads the JSON representation of a <see cref="CloudEventSuccessPayload" />.
    /// </summary>
    public override CloudEventSuccessPayload Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var httpPayload = ResultJsonReader.ReadSuccessPayload(ref reader);
        return new CloudEventSuccessPayload(httpPayload.Metadata);
    }

    /// <summary>
    /// Writing is not supported by this converter.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        CloudEventSuccessPayload value,
        JsonSerializerOptions options
    ) =>
        throw new NotSupportedException(
            $"{nameof(CloudEventSuccessPayloadJsonConverter)} supports deserialization only. Use a serialization converter for writing."
        );
}
