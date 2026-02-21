using System;
using System.Buffers;
using System.Text.Json;
using Light.Results.Buffers;
using Light.Results.SharedJsonSerialization;

namespace Light.Results.CloudEvents.Writing;

/// <summary>
/// Configures how Light.Results values are serialized to CloudEvents JSON envelopes.
/// </summary>
public sealed record LightResultsCloudEventsWriteOptions
{
    /// <summary>
    /// Gets the default options instance for CloudEvent serialization.
    /// </summary>
    public static LightResultsCloudEventsWriteOptions Default { get; } = new ();

    /// <summary>
    /// Gets or sets the default source URI-reference used when no source is provided per call.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets when result metadata should be serialized in CloudEvent data payloads.
    /// </summary>
    public MetadataSerializationMode MetadataSerializationMode { get; set; } = MetadataSerializationMode.Always;

    /// <summary>
    /// Gets or sets serializer options used for result value serialization.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = Module.DefaultSerializerOptions;

    /// <summary>
    /// Gets or sets the conversion service used to map metadata entries to CloudEvent attributes.
    /// </summary>
    public ICloudEventsAttributeConversionService ConversionService { get; set; } =
        DefaultCloudEventsAttributeConversionService.Instance;

    /// <summary>
    /// Gets or sets the CloudEvent type for successful results. Used by STJ converters.
    /// </summary>
    public string? SuccessType { get; set; }

    /// <summary>
    /// Gets or sets the CloudEvent type for failed results. Used by STJ converters.
    /// </summary>
    public string? FailureType { get; set; }

    /// <summary>
    /// Gets or sets the CloudEvent subject. Used by STJ converters.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the CloudEvent data schema URI. Used by STJ converters.
    /// </summary>
    public string? DataSchema { get; set; }

    /// <summary>
    /// Gets or sets the CloudEvent time. When null, UTC now is used. Used by STJ converters.
    /// </summary>
    public DateTimeOffset? Time { get; set; }

    /// <summary>
    /// Gets or sets a factory function to generate unique CloudEvent IDs.
    /// Defaults to generating a new GUID string.
    /// </summary>
    public Func<string>? IdResolver { get; set; }

    /// <summary>
    /// Gets or sets the array pool used for writing CloudEvents.
    /// </summary>
    public ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;

    /// <summary>
    /// Gets or sets the initial array capacity used for writing CloudEvents. The default is 2048 bytes,
    /// see <see cref="Buffers.PooledByteBufferWriter.DefaultInitialCapacity" />.
    /// </summary>
    public int PooledArrayInitialCapacity { get; set; } = PooledByteBufferWriter.DefaultInitialCapacity;
}
