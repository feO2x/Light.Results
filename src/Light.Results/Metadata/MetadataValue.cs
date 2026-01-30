using System;
using System.Globalization;

namespace Light.Results.Metadata;

/// <summary>
/// Represents a JSON-compatible metadata value. This is a discriminated union
/// that can hold null, boolean, int64, double, string, array, or object values.
/// </summary>
public readonly struct MetadataValue : IEquatable<MetadataValue>
{
    private readonly MetadataPayload _payload;

    private MetadataValue(
        MetadataKind kind,
        MetadataPayload payload,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    )
    {
        Kind = kind;
        _payload = payload;
        Annotation = annotation;
    }

    public MetadataKind Kind { get; }

    /// <summary>
    /// Gets the annotation that specifies where this value should be serialized in HTTP responses.
    /// </summary>
    public MetadataValueAnnotation Annotation { get; }

    public static MetadataValue Null => new (MetadataKind.Null, default);

    public static MetadataValue FromBoolean(
        bool value,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    ) =>
        new (MetadataKind.Boolean, new MetadataPayload(value ? 1L : 0L), annotation);

    public static MetadataValue FromInt64(
        long value,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    ) =>
        new (MetadataKind.Int64, new MetadataPayload(value), annotation);

    public static MetadataValue FromDouble(
        double value,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    )
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentException("NaN and Infinity are not allowed in metadata values.", nameof(value));
        }

        return new MetadataValue(MetadataKind.Double, new MetadataPayload(value), annotation);
    }

    public static MetadataValue FromString(
        string? value,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    ) =>
        value is null ? Null : new MetadataValue(MetadataKind.String, new MetadataPayload(value), annotation);

    public static MetadataValue FromDecimal(
        decimal value,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    )
    {
        var @string = value.ToString(CultureInfo.InvariantCulture);
        return new MetadataValue(MetadataKind.String, new MetadataPayload(@string), annotation);
    }

    public static MetadataValue FromArray(
        MetadataArray array,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    )
    {
        ValidateArrayAnnotation(array, annotation);
        return new MetadataValue(MetadataKind.Array, new MetadataPayload(array.Data), annotation);
    }

    public static MetadataValue FromObject(
        MetadataObject @object,
        MetadataValueAnnotation annotation = MetadataValueAnnotation.SerializeInHttpResponseBody
    )
    {
        if ((annotation & MetadataValueAnnotation.SerializeInHttpHeader) != 0)
        {
            throw new ArgumentException(
                "Objects cannot be serialized as HTTP headers. Use SerializeInHttpResponseBody instead.",
                nameof(annotation)
            );
        }

        return new MetadataValue(MetadataKind.Object, new MetadataPayload(@object.Data), annotation);
    }

    private static void ValidateArrayAnnotation(MetadataArray array, MetadataValueAnnotation annotation)
    {
        if ((annotation & MetadataValueAnnotation.SerializeInHttpHeader) == 0 || array.HasOnlyPrimitiveChildren)
        {
            return;
        }

        throw new ArgumentException(
            "Arrays containing nested arrays or objects cannot be serialized as HTTP headers.",
            nameof(annotation)
        );
    }

    // Implicit conversions for ergonomics
    public static implicit operator MetadataValue(bool value) => FromBoolean(value);
    public static implicit operator MetadataValue(int value) => FromInt64(value);
    public static implicit operator MetadataValue(long value) => FromInt64(value);
    public static implicit operator MetadataValue(float value) => FromDouble(value);
    public static implicit operator MetadataValue(double value) => FromDouble(value);
    public static implicit operator MetadataValue(decimal value) => FromDecimal(value);
    public static implicit operator MetadataValue(string? value) => FromString(value);
    public static implicit operator MetadataValue(MetadataArray array) => FromArray(array);
    public static implicit operator MetadataValue(MetadataObject obj) => FromObject(obj);

    // Typed getters
    public bool IsNull => Kind == MetadataKind.Null;

    public bool TryGetBoolean(out bool value)
    {
        if (Kind == MetadataKind.Boolean)
        {
            value = _payload.Int64 != 0;
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetInt64(out long value)
    {
        if (Kind == MetadataKind.Int64)
        {
            value = _payload.Int64;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetDouble(out double value)
    {
        if (Kind == MetadataKind.Double)
        {
            value = _payload.Float64;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetString(out string? value)
    {
        if (Kind == MetadataKind.String)
        {
            value = (string?) _payload.Reference;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetDecimal(out decimal value)
    {
        switch (Kind)
        {
            case MetadataKind.String when _payload.Reference is string @string:
                return decimal.TryParse(@string, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            case MetadataKind.Double:
                try
                {
                    value = (decimal) _payload.Float64;
                    return true;
                }
                catch (OverflowException)
                {
                    value = 0;
                    return false;
                }

            case MetadataKind.Int64:
                value = _payload.Int64;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    public bool TryGetArray(out MetadataArray value)
    {
        if (Kind == MetadataKind.Array && _payload.Reference is MetadataArrayData data)
        {
            value = new MetadataArray(data);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetObject(out MetadataObject value)
    {
        if (Kind == MetadataKind.Object && _payload.Reference is MetadataObjectData data)
        {
            value = new MetadataObject(data);
            return true;
        }

        value = default;
        return false;
    }

    public MetadataArray AsArray() =>
        TryGetArray(out var arr) ? arr : throw new InvalidOperationException($"Cannot convert {Kind} to Array.");

    public MetadataObject AsObject() =>
        TryGetObject(out var obj) ? obj : throw new InvalidOperationException($"Cannot convert {Kind} to Object.");

    public bool Equals(MetadataValue other)
    {
        if (Kind != other.Kind)
        {
            return false;
        }

        return Kind switch
        {
            MetadataKind.Null => true,
            MetadataKind.Boolean or MetadataKind.Int64 => _payload.Int64 == other._payload.Int64,
            MetadataKind.Double => _payload.Float64.Equals(other._payload.Float64),
            MetadataKind.String => string.Equals(
                (string?) _payload.Reference,
                (string?) other._payload.Reference,
                StringComparison.Ordinal
            ),
            MetadataKind.Array => ((MetadataArrayData?) _payload.Reference)?.Equals(
                                      (MetadataArrayData?) other._payload.Reference
                                  ) ??
                                  false,
            MetadataKind.Object => ((MetadataObjectData?) _payload.Reference)?.Equals(
                                       (MetadataObjectData?) other._payload.Reference
                                   ) ??
                                   false,
            _ => false
        };
    }

    public override bool Equals(object? obj) => obj is MetadataValue other && Equals(other);

    public override int GetHashCode()
    {
        var hashCodeBuilder = new HashCode();
        hashCodeBuilder.Add(Kind);
        switch (Kind)
        {
            case MetadataKind.Null:
                break;
            case MetadataKind.Boolean or MetadataKind.Int64:
                hashCodeBuilder.Add(_payload.Int64);
                break;
            case MetadataKind.Double:
                hashCodeBuilder.Add(_payload.Float64);
                break;
            case MetadataKind.String or MetadataKind.Array or MetadataKind.Object:
                hashCodeBuilder.Add(_payload.Reference?.GetHashCode() ?? 0);
                break;
        }

        return hashCodeBuilder.ToHashCode();
    }

    public static bool operator ==(MetadataValue left, MetadataValue right) => left.Equals(right);
    public static bool operator !=(MetadataValue left, MetadataValue right) => !left.Equals(right);

    public override string ToString() =>
        Kind switch
        {
            MetadataKind.Null => "null",
            MetadataKind.Boolean => _payload.Int64 != 0 ? "true" : "false",
            MetadataKind.Int64 => _payload.Int64.ToString(CultureInfo.InvariantCulture),
            MetadataKind.Double => _payload.Float64.ToString(CultureInfo.InvariantCulture),
            MetadataKind.String => $"\"{_payload.Reference}\"",
            MetadataKind.Array =>
                ((MetadataArrayData?) _payload.Reference)?.ToString() ?? MetadataArray.EmptyArrayStringRepresentation,
            MetadataKind.Object => "{...}",
            _ => throw new InvalidOperationException($"Kind '{Kind}' is unknown")
        };
}
