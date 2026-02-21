using System;
using System.Buffers;
using System.Collections.Generic;
using FluentAssertions;
using Light.Results.Buffers;
using Xunit;

namespace Light.Results.Tests.Buffers;

public sealed class PooledArrayTests
{
    [Fact]
    public void Constructor_WithNegativeLength_ShouldThrowArgumentOutOfRangeException()
    {
        var pool = new TrackingByteArrayPool();

        var act = () => _ = new PooledArray(new byte[4], pool, -1);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("length");
    }

    [Fact]
    public void Constructor_WithNullRentedArray_ShouldThrowArgumentNullException()
    {
        var pool = new TrackingByteArrayPool();

        var act = () => _ = new PooledArray(null!, pool, 0);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("rentedArray");
    }

    [Fact]
    public void Constructor_WithNullArrayPool_ShouldThrowArgumentNullException()
    {
        var act = () => _ = new PooledArray(new byte[1], null!, 1);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("arrayPool");
    }

    [Fact]
    public void AsMemory_ShouldReturnRequestedLength()
    {
        var pool = new TrackingByteArrayPool();
        var rentedArray = new byte[] { 1, 2, 3, 4, 5 };
        var pooledArray = new PooledArray(rentedArray, pool, 3);

        var memory = pooledArray.AsMemory();

        memory.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AsSpan_ShouldReturnRequestedLength()
    {
        var pool = new TrackingByteArrayPool();
        var rentedArray = new byte[] { 1, 2, 3, 4, 5 };
        var pooledArray = new PooledArray(rentedArray, pool, 2);

        var span = pooledArray.AsSpan();

        span.ToArray().Should().Equal(1, 2);
    }

    [Fact]
    public void Dispose_ShouldReturnArrayToPoolOnlyOnce()
    {
        var pool = new TrackingByteArrayPool();
        var rentedArray = new byte[] { 10, 20, 30 };
        var pooledArray = new PooledArray(rentedArray, pool, 3);

        pooledArray.Dispose();
        pooledArray.Dispose();

        pool.ReturnCalls.Should().Be(1);
        pool.ReturnedArrays.Should().ContainSingle().Which.Should().BeSameAs(rentedArray);
    }

    [Fact]
    public void AsMemory_AfterDispose_ShouldThrowInvalidOperationException()
    {
        var pool = new TrackingByteArrayPool();
        var pooledArray = new PooledArray(new byte[] { 1, 2 }, pool, 2);
        pooledArray.Dispose();

        var act = () => _ = pooledArray.AsMemory();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already disposed*");
    }

    private sealed class TrackingByteArrayPool : ArrayPool<byte>
    {
        public int ReturnCalls { get; private set; }

        public List<byte[]> ReturnedArrays { get; } = new ();

        public override byte[] Rent(int minimumLength) => new byte[Math.Max(minimumLength, 1)];

        public override void Return(byte[] array, bool clearArray = false)
        {
            ReturnCalls++;
            ReturnedArrays.Add(array);
        }
    }
}
