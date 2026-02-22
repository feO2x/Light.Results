using System;

namespace Light.Results.Buffers;

/// <summary>
/// Represents the abstraction of a rented array. When <see cref="IDisposable.Dispose" /> is called, the underlying
/// rented array will be returned to the corresponding array pool.
/// </summary>
public interface IRentedArray : IDisposable
{
    /// <summary>
    /// Gets a ReadOnlyMemory&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the PooledArray has been disposed.</exception>
    ReadOnlyMemory<byte> Memory { get; }

    /// <summary>
    /// Gets a ReadOnlySpan&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <returns>A ReadOnlySpan&lt;byte> wrapping the pooled array data.</returns>
    ReadOnlySpan<byte> Span { get; }
}
