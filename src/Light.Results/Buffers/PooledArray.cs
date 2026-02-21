using System;
using System.Buffers;

namespace Light.Results.Buffers;

/// <summary>
/// Represents a rented array from an ArrayPool that provides read-only access to the data.
/// When disposed, the array is returned to the pool for reuse.
/// </summary>
public record struct PooledArray : IDisposable
{
    private readonly ArrayPool<byte> _arrayPool;
    private readonly int _length;
    private byte[]? _rentedArray;

    /// <summary>
    /// Initializes a new instance of the PooledArray struct.
    /// </summary>
    /// <param name="rentedArray">The rented array from the pool.</param>
    /// <param name="arrayPool">The ArrayPool to return the array to when disposed.</param>
    /// <param name="length">The length of valid data in the array.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when length is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown when rentedArray or arrayPool is null.</exception>
    public PooledArray(byte[] rentedArray, ArrayPool<byte> arrayPool, int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        _rentedArray = rentedArray ?? throw new ArgumentNullException(nameof(rentedArray));
        _arrayPool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
        _length = length;
    }

    /// <summary>
    /// Returns the rented array to the ArrayPool and marks this PooledArray as disposed.
    /// This method is idempotent and can be called multiple times safely.
    /// </summary>
    public void Dispose()
    {
        if (_rentedArray is null)
        {
            return;
        }

        _arrayPool.Return(_rentedArray);
        _rentedArray = null;
    }

    /// <summary>
    /// Gets a ReadOnlyMemory&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <returns>A ReadOnlyMemory&lt;byte> /> wrapping the pooled array data.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the PooledArray has been disposed.</exception>
    public ReadOnlyMemory<byte> AsMemory()
    {
        if (_rentedArray is null)
        {
            throw new InvalidOperationException("The PooledArray is already disposed");
        }

        return _rentedArray?.AsMemory(0, _length) ?? ReadOnlyMemory<byte>.Empty;
    }

    /// <summary>
    /// Gets a ReadOnlySpan&lt;byte> that provides read-only access to the pooled array data.
    /// </summary>
    /// <returns>A ReadOnlySpan&lt;byte> wrapping the pooled array data.</returns>
    public ReadOnlySpan<byte> AsSpan() => AsMemory().Span;
}
