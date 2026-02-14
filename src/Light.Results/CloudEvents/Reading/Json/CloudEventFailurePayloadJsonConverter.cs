using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// JSON converter for reading <see cref="CloudEventFailurePayload" /> from CloudEvent data payloads.
/// </summary>
public sealed class CloudEventFailurePayloadJsonConverter : JsonConverter<CloudEventFailurePayload>
{
    /// <summary>
    /// Reads the JSON representation of a <see cref="CloudEventFailurePayload" />.
    /// </summary>
    public override CloudEventFailurePayload Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return CloudEventDataJsonReader.ReadFailurePayload(ref reader);
    }

    /// <summary>
    /// Writing is not supported by this converter.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        CloudEventFailurePayload value,
        JsonSerializerOptions options
    ) =>
        throw new NotSupportedException(
            $"{nameof(CloudEventFailurePayloadJsonConverter)} supports deserialization only. Use a serialization converter for writing."
        );
}
