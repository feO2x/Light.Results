using System;
using System.Buffers;
using System.Collections.Generic;

namespace Light.Results.Metadata;

/// <summary>
/// Builder for creating <see cref="MetadataObject" /> instances efficiently using pooled buffers.
/// Entries are stored in insertion order.
/// </summary>
public struct MetadataObjectBuilder : IDisposable
{
    private const int DefaultCapacity = 4;
    private const int DictionaryThreshold = 10;

    private KeyValuePair<string, MetadataValue>[]? _entries;
    private Dictionary<string, int>? _indexLookup;
    private IEqualityComparer<string>? _keyComparer;
    private bool _built;

    public int Count { get; private set; }

    public static MetadataObjectBuilder Create(int capacity = DefaultCapacity)
    {
        var actualCapacity = Math.Max(capacity, DefaultCapacity);
        var builder = new MetadataObjectBuilder
        {
            _entries = ArrayPool<KeyValuePair<string, MetadataValue>>.Shared.Rent(actualCapacity),
            Count = 0,
            _built = false
        };
        return builder;
    }

    public static MetadataObjectBuilder From(MetadataObject source)
    {
        if (source.Data is null || source.Count == 0)
        {
            return Create();
        }

        var builder = Create(source.Count);
        var sourceEntries = source.Data.GetEntries();

        Array.Copy(sourceEntries, builder._entries!, sourceEntries.Length);
        builder.Count = source.Count;

        return builder;
    }

    public void Add(string key, MetadataValue value)
    {
        ThrowIfBuilt();

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (ContainsKey(key))
        {
            throw new ArgumentException($"Duplicate key: '{key}'.", nameof(key));
        }

        EnsureCapacity(Count + 1);

        _entries![Count] = new KeyValuePair<string, MetadataValue>(key, value);

        // Update dictionary if it exists
        if (_indexLookup is not null)
        {
            _indexLookup[key] = Count;
        }

        Count++;
    }

    public bool TryGetValue(string key, out MetadataValue value)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var index = FindIndex(key);
        if (index >= 0)
        {
            value = _entries![index].Value;
            return true;
        }

        value = default;
        return false;
    }

    public bool ContainsKey(string key)
    {
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        return FindIndex(key) >= 0;
    }

    public void Replace(string key, MetadataValue value)
    {
        ThrowIfBuilt();

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var index = FindIndex(key);
        if (index < 0)
        {
            throw new KeyNotFoundException($"Key '{key}' not found.");
        }

        _entries![index] = new KeyValuePair<string, MetadataValue>(key, value);
    }

    public void AddOrReplace(string key, MetadataValue value)
    {
        ThrowIfBuilt();

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var existingIndex = FindIndex(key);
        if (existingIndex >= 0)
        {
            // Key exists, replace value
            _entries![existingIndex] = new KeyValuePair<string, MetadataValue>(key, value);
            return;
        }

        // Append new entry
        EnsureCapacity(Count + 1);

        _entries![Count] = new KeyValuePair<string, MetadataValue>(key, value);

        // Update dictionary if it exists
        if (_indexLookup is not null)
        {
            _indexLookup[key] = Count;
        }

        Count++;
    }

    public void SetKeyComparer(IEqualityComparer<string> keyComparer)
    {
        ThrowIfBuilt();
        _keyComparer = keyComparer;
    }

    public MetadataObject Build()
    {
        ThrowIfBuilt();
        _built = true;

        if (Count == 0)
        {
            ReturnBuffer();
            return MetadataObject.Empty;
        }

        var entries = new KeyValuePair<string, MetadataValue>[Count];
        Array.Copy(_entries!, entries, Count);

        // Entries are already sorted due to sorted insertion in Add/AddOrReplace
        ReturnBuffer();

        return new MetadataObject(new MetadataObjectData(entries, _keyComparer));
    }

    public void Dispose()
    {
        if (_built)
        {
            return;
        }

        ReturnBuffer();
        _built = true;
    }

    private void EnsureCapacity(int required)
    {
        if (_entries is null || _entries.Length >= required)
        {
            if (_entries is null)
            {
                var capacity = Math.Max(required, DefaultCapacity);
                _entries = ArrayPool<KeyValuePair<string, MetadataValue>>.Shared.Rent(capacity);
            }

            return;
        }

        var newCapacity = Math.Max(_entries.Length * 2, required);
        var newEntries = ArrayPool<KeyValuePair<string, MetadataValue>>.Shared.Rent(newCapacity);

        Array.Copy(_entries, newEntries, Count);

        ArrayPool<KeyValuePair<string, MetadataValue>>.Shared.Return(_entries, clearArray: true);

        _entries = newEntries;
    }

    private void ReturnBuffer()
    {
        if (_entries is not null)
        {
            ArrayPool<KeyValuePair<string, MetadataValue>>.Shared.Return(_entries, clearArray: true);
            _entries = null;
        }
    }

    private void ThrowIfBuilt()
    {
        if (_built)
        {
            throw new InvalidOperationException("Builder has already been used to build an object.");
        }
    }

    /// <summary>
    /// Finds the index of an existing key using linear search or dictionary lookup.
    /// Returns the index if found, or -1 if not found.
    /// </summary>
    private int FindIndex(string key)
    {
        if (Count == 0)
        {
            return -1;
        }

        if (Count > DictionaryThreshold)
        {
            return FindIndexWithDictionary(key);
        }

        return FindIndexLinear(key);
    }

    private int FindIndexLinear(string key)
    {
        if (_keyComparer is null)
        {
            for (var i = 0; i < Count; i++)
            {
                if (key.Equals(_entries![i].Key, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        for (var i = 0; i < Count; i++)
        {
            if (_keyComparer.Equals(key, _entries![i].Key))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindIndexWithDictionary(string key)
    {
        if (_indexLookup is null)
        {
            _indexLookup = _keyComparer is null ?
                new Dictionary<string, int>(Count) :
                new Dictionary<string, int>(Count, _keyComparer);

            for (var i = 0; i < Count; i++)
            {
                _indexLookup[_entries![i].Key] = i;
            }
        }

        return _indexLookup.TryGetValue(key, out var index) ? index : -1;
    }
}
