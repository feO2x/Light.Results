using System;

// ReSharper disable ConvertToExtensionBlock

namespace Light.Results.Metadata;

/// <summary>
/// Extension methods for <see cref="MetadataObject" /> including merge operations.
/// </summary>
public static class MetadataObjectExtensions
{
    /// <summary>
    /// Merges metadata only if needed, returning the existing reference when incoming is empty
    /// or references are equal.
    /// </summary>
    /// <param name="existing">The existing metadata (can be null).</param>
    /// <param name="incoming">The incoming metadata (can be null).</param>
    /// <param name="strategy">The merge strategy to use.</param>
    /// <returns>The merged metadata, or the existing/incoming reference if no merge is needed.</returns>
    public static MetadataObject? MergeIfNeeded(
        MetadataObject? existing,
        MetadataObject? incoming,
        MetadataMergeStrategy strategy = MetadataMergeStrategy.AddOrReplace
    )
    {
        if (incoming is null || incoming.Value.Count == 0)
        {
            return existing;
        }

        if (existing is null || existing.Value.Count == 0)
        {
            return incoming;
        }

        // Check if both dictionaries contain the same content - only merge if this is not the case
        return existing.Value == incoming.Value ? existing : existing.Value.Merge(incoming.Value, strategy);
    }

    /// <summary>
    /// Merges two <see cref="MetadataObject" /> instances according to the specified strategy.
    /// </summary>
    public static MetadataObject Merge(
        this MetadataObject original,
        MetadataObject incoming,
        MetadataMergeStrategy strategy = MetadataMergeStrategy.AddOrReplace
    )
    {
        if (incoming.Count == 0)
        {
            return original;
        }

        if (original.Count == 0)
        {
            return incoming;
        }

        // Skip merge if references are equal
        if (original == incoming)
        {
            return original;
        }

        using var builder = MetadataObjectBuilder.From(original);

        foreach (var kvp in incoming)
        {
            var key = kvp.Key;
            var incomingValue = kvp.Value;

            if (!builder.TryGetValue(key, out var existingValue))
            {
                builder.Add(key, incomingValue);
                continue;
            }

            switch (strategy)
            {
                case MetadataMergeStrategy.AddOrReplace:
                    builder.Replace(key, MergeValues(existingValue, incomingValue, strategy));
                    break;

                case MetadataMergeStrategy.PreserveExisting:
                    // Keep existing, do nothing
                    break;

                case MetadataMergeStrategy.FailOnConflict:
                    throw new InvalidOperationException($"Duplicate metadata key '{key}'.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown merge strategy.");
            }
        }

        return builder.Build();
    }

    private static MetadataValue MergeValues(
        MetadataValue left,
        MetadataValue right,
        MetadataMergeStrategy strategy
    )
    {
        if (left.Kind == MetadataKind.Object &&
            right.Kind == MetadataKind.Object &&
            left.TryGetObject(out var leftObj) &&
            right.TryGetObject(out var rightObj))
        {
            return MetadataValue.FromObject(leftObj.Merge(rightObj, strategy));
        }

        // Scalars and arrays are replaced wholesale
        return right;
    }

    /// <summary>
    /// Creates a new <see cref="MetadataObject" /> with an additional property.
    /// </summary>
    public static MetadataObject With(this MetadataObject metadata, string key, MetadataValue value)
    {
        using var builder = MetadataObjectBuilder.From(metadata);
        builder.AddOrReplace(key, value);
        return builder.Build();
    }

    /// <summary>
    /// Creates a new <see cref="MetadataObject" /> with additional properties.
    /// </summary>
    public static MetadataObject With(
        this MetadataObject metadata,
        params (string Key, MetadataValue Value)[]? properties
    )
    {
        if (properties is null || properties.Length == 0)
        {
            return metadata;
        }

        using var builder = MetadataObjectBuilder.From(metadata);
        foreach (var (key, value) in properties)
        {
            builder.AddOrReplace(key, value);
        }

        return builder.Build();
    }
}
