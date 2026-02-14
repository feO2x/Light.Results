using System;
using Light.Results.Metadata;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// Represents a parsed CloudEvent envelope with raw data bytes for deferred payload deserialization.
/// </summary>
public readonly struct CloudEventEnvelopePayload
{
    /// <summary>
    /// Initializes a new instance of <see cref="CloudEventEnvelopePayload" />.
    /// </summary>
    public CloudEventEnvelopePayload(
        string type,
        string source,
        string id,
        string? subject,
        DateTimeOffset? time,
        string? dataContentType,
        string? dataSchema,
        MetadataObject? extensionAttributes,
        bool hasData,
        bool isDataNull,
        byte[]? dataBytes
    )
    {
        Type = type;
        Source = source;
        Id = id;
        Subject = subject;
        Time = time;
        DataContentType = dataContentType;
        DataSchema = dataSchema;
        ExtensionAttributes = extensionAttributes;
        HasData = hasData;
        IsDataNull = isDataNull;
        DataBytes = dataBytes;
    }

    /// <summary>
    /// Gets the CloudEvent type attribute.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the CloudEvent source attribute.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the CloudEvent id attribute.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the optional CloudEvent subject attribute.
    /// </summary>
    public string? Subject { get; }

    /// <summary>
    /// Gets the optional CloudEvent time attribute.
    /// </summary>
    public DateTimeOffset? Time { get; }

    /// <summary>
    /// Gets the optional CloudEvent datacontenttype attribute.
    /// </summary>
    public string? DataContentType { get; }

    /// <summary>
    /// Gets the optional CloudEvent dataschema attribute.
    /// </summary>
    public string? DataSchema { get; }

    /// <summary>
    /// Gets the parsed extension attributes.
    /// </summary>
    public MetadataObject? ExtensionAttributes { get; }

    /// <summary>
    /// Gets a value indicating whether the data property was present in the envelope.
    /// </summary>
    public bool HasData { get; }

    /// <summary>
    /// Gets a value indicating whether the data property was null.
    /// </summary>
    public bool IsDataNull { get; }

    /// <summary>
    /// Gets the raw UTF-8 bytes of the data payload for deferred deserialization.
    /// </summary>
    public byte[]? DataBytes { get; }
}
