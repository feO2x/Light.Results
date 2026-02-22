using Light.Results.Metadata;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// Represents a deserialized successful non-generic CloudEvents data payload.
/// </summary>
public readonly struct CloudEventSuccessPayload
{
    /// <summary>
    /// Initializes a new instance of <see cref="CloudEventSuccessPayload" />.
    /// </summary>
    public CloudEventSuccessPayload(MetadataObject? metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the metadata parsed from the payload.
    /// </summary>
    public MetadataObject? Metadata { get; }
}
