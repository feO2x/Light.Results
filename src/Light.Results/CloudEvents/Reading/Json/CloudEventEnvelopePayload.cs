using System;
using Light.Results.Metadata;

namespace Light.Results.CloudEvents.Reading.Json;

/// <summary>
/// Represents a parsed CloudEvent envelope with indicators for the Cloud Events <c>data</c> property.
/// </summary>
/// <param name="Type">The CloudEvent type attribute.</param>
/// <param name="Source">The CloudEvent source attribute.</param>
/// <param name="Id">The CloudEvent id attribute.</param>
/// <param name="Subject">The optional CloudEvent subject attribute.</param>
/// <param name="Time">The optional CloudEvent time attribute.</param>
/// <param name="DataContentType">The optional CloudEvent datacontenttype attribute.</param>
/// <param name="DataSchema">The optional CloudEvent dataschema attribute.</param>
/// <param name="ExtensionAttributes">The parsed extension attributes.</param>
/// <param name="HasData">A value indicating whether the data property was present in the envelope.</param>
/// <param name="IsDataNull">A value indicating whether the data property was null.</param>
/// <param name="DataStart">The byte offset where the data value begins in the original buffer.</param>
/// <param name="DataLength">The length of the data value in bytes.</param>
public readonly record struct CloudEventEnvelopePayload(
    string Type,
    string Source,
    string Id,
    string? Subject,
    DateTimeOffset? Time,
    string? DataContentType,
    string? DataSchema,
    MetadataObject? ExtensionAttributes,
    bool HasData,
    bool IsDataNull,
    int DataStart,
    int DataLength
);
