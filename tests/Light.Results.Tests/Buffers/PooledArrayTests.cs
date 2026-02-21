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
    public void AsMemory_ShouldReturnRequestedLength()
    {
        var pool = new TrackingByteArrayPool();
        using var pooledArray = CreatePooledArrayWithLength(pool, 3, 1, 2, 3, 4, 5);

        var memory = pooledArray.AsMemory();

        memory.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AsSpan_ShouldReturnRequestedLength()
    {
        var pool = new TrackingByteArrayPool();
        using var pooledArray = CreatePooledArray(pool, 1, 2, 3, 4, 5);

        var span = pooledArray.AsSpan();

        span.ToArray().Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void Dispose_ShouldReturnArrayToPoolOnlyOnce()
    {
        var pool = new TrackingByteArrayPool();
        var pooledArray = CreatePooledArray(pool, 10, 20, 30);

        pooledArray.Dispose();
        pooledArray.Dispose();

        pool.ReturnCalls.Should().Be(1);
        pool.ReturnedArrays.Should().ContainSingle();
    }

    [Fact]
    public void Dispose_WhenStructCopied_ShouldReturnArrayToPoolOnlyOnce()
    {
        var pool = new TrackingByteArrayPool();
        var pooledArray = CreatePooledArray(pool, 10, 20, 30);
        var copied = pooledArray;

        pooledArray.Dispose();
        copied.Dispose();

        pool.ReturnCalls.Should().Be(1);
        pool.ReturnedArrays.Should().ContainSingle();
    }

    [Fact]
    public void AsMemory_AfterDispose_ShouldThrowInvalidOperationException()
    {
        var pool = new TrackingByteArrayPool();
        var pooledArray = CreatePooledArray(pool, 1, 2);
        pooledArray.Dispose();

        var act = () => _ = pooledArray.AsMemory();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already disposed*");
    }

    private static PooledArray CreatePooledArray(ArrayPool<byte> pool, params byte[] bytes)
        => CreatePooledArrayWithLength(pool, bytes.Length, bytes);

    private static PooledArray CreatePooledArrayWithLength(ArrayPool<byte> pool, int length, params byte[] bytes)
    {
        var writer = new PooledByteBufferWriter(pool, bytes.Length);
        bytes.CopyTo(writer.GetSpan(bytes.Length));
        writer.Advance(length);
        return writer.ToPooledArray();
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
