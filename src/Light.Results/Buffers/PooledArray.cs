using System;

namespace Light.Results.Buffers;

/// <summary>
/// Represents a rented array from an ArrayPool that provides read-only access to the data.
/// When disposed, the array is returned to the pool for reuse.
/// </summary>
public readonly record struct PooledArray : IDisposable
{
    private readonly PooledByteBufferWriter _owner;

    /// <summary>
    /// Initializes a new instance of the PooledArray struct.
    /// </summary>
    /// <param name="owner">The writer that owns the pooled buffer and disposal state.</param>
    /// <exception cref="ArgumentNullException">Thrown when owner is null.</exception>
    public PooledArray(PooledByteBufferWriter owner) =>
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

    /// <summary>
    /// Returns the rented array to the ArrayPool and marks this PooledArray as disposed.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    public void Dispose() => _owner.Dispose();

    /// <summary>
    /// Gets a ReadOnlyMemory&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <returns>A ReadOnlyMemory&lt;byte> /> wrapping the pooled array data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the PooledArray has been disposed.</exception>
    public ReadOnlyMemory<byte> AsMemory() => _owner.PooledArrayAsMemory();

    /// <summary>
    /// Gets a ReadOnlySpan&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <returns>A ReadOnlySpan&lt;byte> wrapping the pooled array data.</returns>
    public ReadOnlySpan<byte> AsSpan() => AsMemory().Span;
}
