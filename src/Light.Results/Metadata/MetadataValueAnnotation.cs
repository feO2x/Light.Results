using System;

namespace Light.Results.Metadata;

/// <summary>
/// Specifies where a metadata value should be serialized in HTTP responses.
/// </summary>
[Flags]
public enum MetadataValueAnnotation
{
    /// <summary>
    /// No annotation specified. Metadata will not be serialized.
    /// </summary>
    None = 0,

    /// <summary>
    /// Serialize this value in the HTTP response body.
    /// </summary>
    SerializeInHttpResponseBody = 1,

    /// <summary>
    /// Serialize this value as an HTTP response header.
    /// Only valid for primitive types and arrays of primitives.
    /// </summary>
    SerializeInHttpHeader = 2,

    /// <summary>
    /// Serialize this value in both the HTTP response body and as a header.
    /// </summary>
    SerializeInHttpHeaderAndBody = SerializeInHttpResponseBody | SerializeInHttpHeader
}
