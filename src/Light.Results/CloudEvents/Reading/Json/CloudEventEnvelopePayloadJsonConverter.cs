using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// JSON converter for reading <see cref="CloudEventEnvelopePayload" /> from CloudEvents JSON envelopes.
/// </summary>
public sealed class CloudEventEnvelopePayloadJsonConverter : JsonConverter<CloudEventEnvelopePayload>
{
    /// <summary>
    /// Reads the JSON representation of a <see cref="CloudEventEnvelopePayload" />.
    /// </summary>
    public override CloudEventEnvelopePayload Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return CloudEventEnvelopeJsonReader.ReadEnvelope(ref reader);
    }

    /// <summary>
    /// Writing is not supported by this converter.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        CloudEventEnvelopePayload value,
        JsonSerializerOptions options
    ) =>
        throw new NotSupportedException(
            $"{nameof(CloudEventEnvelopePayloadJsonConverter)} supports deserialization only. Use a serialization converter for writing."
        );
}
