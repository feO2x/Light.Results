using System;
using System.Collections;
using System.Collections.Generic;

namespace Light.Results;

/// <summary>
/// Stores one or more errors with small-buffer optimization.
/// Single error is stored inline; multiple errors use a <see cref="ReadOnlyMemory{T}" />.
/// Implements <see cref="IReadOnlyList{T}" /> with a zero-allocation value-type enumerator.
/// </summary>
public readonly struct Errors : IReadOnlyList<Error>
{
    private readonly Error _one;
    private readonly ReadOnlyMemory<Error> _many;

    public int Count { get; }

    public Error this[int index]
    {
        get
        {
            // Use unsigned comparison to fold the index < 0 and index >= Count checks into one branch.
            if ((uint) index >= (uint) Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return Count == 1 ? _one : _many.Span[index];
        }
    }

    public Error First => Count switch
    {
        <= 0 => throw new InvalidOperationException("No errors present."),
        1 => _one,
        _ => _many.Span[0]
    };

    public Errors(Error one)
    {
        _one = one;
        _many = ReadOnlyMemory<Error>.Empty;
        Count = 1;
    }

    public Errors(ReadOnlyMemory<Error> many)
    {
        if (many.IsEmpty)
        {
            _one = default;
            _many = ReadOnlyMemory<Error>.Empty;
            Count = 0;
            return;
        }

        if (many.Length == 1)
        {
            _one = many.Span[0];
            _many = ReadOnlyMemory<Error>.Empty;
            Count = 1;
            return;
        }

        _one = default;
        _many = many;
        Count = many.Length;
    }

    public Enumerator GetEnumerator() => new (this);

    IEnumerator<Error> IEnumerable<Error>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Value-type enumerator that avoids compiler-generated state machine allocations.
    /// </summary>
    public struct Enumerator : IEnumerator<Error>
    {
        private readonly Error _one;
        private readonly ReadOnlyMemory<Error> _many;
        private readonly int _count;
        private int _index;

        public Enumerator(Errors errors)
        {
            _one = errors._one;
            _many = errors._many;
            _count = errors.Count;
            _index = -1;
        }

        public Error Current => _count == 1 ?
            _index == 0 ?
                _one :
                throw new InvalidOperationException(
                    "Enumerator is positioned before the first element or after the last element."
                ) :
            _many.Span[_index];

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
