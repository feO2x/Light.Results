using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.Results.Metadata;

/// <summary>
/// Represents an immutable object with string keys and <see cref="MetadataValue" /> values.
/// Properties are stored in insertion order.
/// </summary>
public readonly struct MetadataObject : IReadOnlyDictionary<string, MetadataValue>, IEquatable<MetadataObject>
{
    public const string EmptyObjectStringRepresentation = "{}";

    internal readonly MetadataObjectData? Data;

    public static MetadataObject Empty => new (MetadataObjectData.Empty);

    internal MetadataObject(MetadataObjectData data) => Data = data;

    public int Count => Data?.Count ?? 0;

    public MetadataValue this[string key] =>
        TryGetValue(key, out var value) ?
            value :
            throw new KeyNotFoundException($"Key '{key}' was not found in MetadataObject");

    public IEnumerable<string> Keys
    {
        get
        {
            if (Data is null)
            {
                yield break;
            }

            for (var i = 0; i < Data.Count; i++)
            {
                yield return Data.GetEntry(i).Key;
            }
        }
    }

    public IEnumerable<MetadataValue> Values
    {
        get
        {
            if (Data is null)
            {
                yield break;
            }

            for (var i = 0; i < Data.Count; i++)
            {
                yield return Data.GetEntry(i).Value;
            }
        }
    }

    /// <summary>
    /// Gets the value indicating whether this object only contains primitive values.
    /// Uses the <see cref="MetadataKindExtensions.IsPrimitive" /> method internally.
    /// </summary>
    public bool HasOnlyPrimitiveChildren => Data?.HasOnlyPrimitiveChildren ?? true;

    public bool ContainsKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return Data is not null && Data.TryGetValue(key, out _);
    }

    public bool TryGetValue(string key, out MetadataValue value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (Data is not null)
        {
            return Data.TryGetValue(key, out value);
        }

        value = default;
        return false;
    }

    // Typed getters for convenience
    public bool TryGetBoolean(string key, out bool value)
    {
        if (TryGetValue(key, out var mv) && mv.TryGetBoolean(out value))
        {
            return true;
        }

        value = false;
        return false;
    }

    public bool TryGetInt64(string key, out long value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetInt64(out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetDouble(string key, out double value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetDouble(out value))
        {
            return true;
        }

        value = 0.0;
        return false;
    }

    public bool TryGetString(string key, out string? value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetString(out value))
        {
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetDecimal(string key, out decimal value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetDecimal(out value))
        {
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetArray(string key, out MetadataArray value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetArray(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetObject(string key, out MetadataObject value)
    {
        if (TryGetValue(key, out var metadataValue) && metadataValue.TryGetObject(out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    public static MetadataObject Create(params (string Key, MetadataValue Value)[]? properties) =>
        Create(null, properties);

    public static MetadataObject Create(
        IEqualityComparer<string>? keyComparer,
        params (string Key, MetadataValue Value)[]? properties
    )
    {
        if (properties is null || properties.Length == 0)
        {
            return Empty;
        }

        var entries = new KeyValuePair<string, MetadataValue>[properties.Length];

        for (var i = 0; i < properties.Length; i++)
        {
            var key = properties[i].Key ?? throw new ArgumentNullException(nameof(properties), "Key cannot be null.");
            entries[i] = new KeyValuePair<string, MetadataValue>(key, properties[i].Value);
        }

        return new MetadataObject(new MetadataObjectData(entries, keyComparer));
    }

    public Enumerator GetEnumerator() => new (this);

    IEnumerator<KeyValuePair<string, MetadataValue>> IEnumerable<KeyValuePair<string, MetadataValue>>.GetEnumerator() =>
        GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(MetadataObject other)
    {
        if (Data is null && other.Data is null)
        {
            return true;
        }

        if (Data is null || other.Data is null)
        {
            return false;
        }

        return Data.Equals(other.Data);
    }

    public override bool Equals(object? obj) => obj is MetadataObject other && Equals(other);

    public override int GetHashCode() => Data?.GetHashCode() ?? 0;

    public static bool operator ==(MetadataObject left, MetadataObject right) => left.Equals(right);
    public static bool operator !=(MetadataObject left, MetadataObject right) => !left.Equals(right);

    public override string ToString() => Data?.ToString() ?? EmptyObjectStringRepresentation;

    public struct Enumerator : IEnumerator<KeyValuePair<string, MetadataValue>>
    {
        private readonly KeyValuePair<string, MetadataValue>[]? _entries;
        private readonly int _count;
        private int _index;

        internal Enumerator(MetadataObject @object)
        {
            if (@object.Data is null)
            {
                _entries = null;
                _count = 0;
            }
            else
            {
                _entries = @object.Data.GetEntries();
                _count = @object.Data.Count;
            }

            _index = -1;
        }

        public KeyValuePair<string, MetadataValue> Current => _entries![_index];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public void Reset() => _index = -1;

        public void Dispose() { }
    }
}
