using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Light.Results;

/// <summary>
/// Represents either a successful value of <typeparamref name="T" /> or one or more errors.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Result<T>
{
    private readonly Errors _errors;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the successful value. Throws if this is a failure.</summary>
    public T Value =>
        IsSuccess ? field! : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    /// <summary>Returns errors as an immutable array (empty on success).</summary>
    public ImmutableArray<Error> ErrorList => IsSuccess ? ImmutableArray<Error>.Empty : _errors.ToImmutableArray();

    /// <summary>Returns the first error (or throws if success).</summary>
    public Error FirstError => IsFailure ?
        _errors.First :
        throw new InvalidOperationException("Cannot access errors on a successful Result.");

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        _errors = default;
    }

    private Result(Errors errors)
    {
        IsSuccess = false;
        Value = default;
        _errors = errors;
    }

    public static Result<T> Ok(T value) => new (value);

    public static Result<T> Fail(Error error) => new (new Errors(error));

    public static Result<T> Fail(IEnumerable<Error> errors)
    {
        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        // Avoid multiple enumeration and keep it compact.
        if (errors is Error[] arr)
        {
            if (arr.Length == 0)
            {
                throw new ArgumentException("At least one error is required.", nameof(errors));
            }

            return new Result<T>(Errors.FromArray(arr));
        }

        var list = errors as IList<Error> ?? errors.ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one error is required.", nameof(errors));
        }

        if (list.Count == 1)
        {
            return Fail(list[0]);
        }

        return new Result<T>(Errors.FromArray(list.ToArray()));
    }

    /// <summary>Transforms the successful value.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
        => IsSuccess ? Result<TOut>.Ok(map(Value)) : Result<TOut>.Fail(_errors);

    /// <summary>Chains another result-returning function.</summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
        => IsSuccess ? bind(Value) : Result<TOut>.Fail(_errors);

    /// <summary>Executes an action on success and returns the same result.</summary>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    /// <summary>Executes an action on failure and returns the same result.</summary>
    public Result<T> TapError(Action<ImmutableArray<Error>> action)
    {
        if (IsFailure)
        {
            action(ErrorList);
        }

        return this;
    }

    public override string ToString()
        => IsSuccess ? $"Ok({Value})" : $"Fail({string.Join(", ", ErrorList.Select(e => e.Code))})";

    private string DebuggerDisplay => IsSuccess ? $"Ok({Value})" : $"Fail({ErrorList.Length} error(s))";

    // Convenience implicit conversions
    public static implicit operator Result<T>(T value) => Ok(value);
    public static implicit operator Result<T>(Error error) => Fail(error);


    // Allow Result<T>.Fail(_errors) reuse without re-allocating arrays.
    private static Result<T> Fail(Errors errors) => new (errors);
}

/// <summary>
/// Non-generic convenience result (success/failure only).
/// </summary>
public readonly struct Result
{
    private readonly Result<Unit> _inner;
    private Result(Result<Unit> inner) => _inner = inner;

    public bool IsSuccess => _inner.IsSuccess;
    public bool IsFailure => _inner.IsFailure;
    public ImmutableArray<Error> ErrorList => _inner.ErrorList;

    public static Result Ok() => new (Result<Unit>.Ok(Unit.Value));
    public static Result Fail(Error error) => new (Result<Unit>.Fail(error));
    public static Result Fail(IEnumerable<Error> errors) => new (Result<Unit>.Fail(errors));
}

/// <summary>
/// Represents a void-like successful value.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new ();
}

// Internal errors storage with small-buffer optimization:
// - one error inline (no array allocation)
// - many errors in an array
public readonly struct Errors : IEnumerable<Error>
{
    private readonly Error _one;
    private readonly Error[]? _many;

    public int Count { get; }

    public Error First => Count switch
    {
        <= 0 => throw new InvalidOperationException("No errors present."),
        1 => _one,
        _ => _many![0]
    };

    public Errors(Error one)
    {
        _one = one;
        _many = null;
        Count = 1;
    }

    private Errors(Error[] many)
    {
        if (many is null)
        {
            throw new ArgumentNullException(nameof(many));
        }

        if (many.Length < 2)
        {
            throw new ArgumentException("Use single-error constructor for one error.", nameof(many));
        }

        _one = default;
        _many = many;
        Count = many.Length;
    }

    public static Errors FromArray(Error[] errors)
        => errors.Length == 1 ? new Errors(errors[0]) : new Errors(errors);

    public ImmutableArray<Error> ToImmutableArray()
    {
        if (Count == 0)
        {
            return ImmutableArray<Error>.Empty;
        }

        if (Count == 1)
        {
            return ImmutableArray.Create(_one);
        }

        return ImmutableArray.Create(_many!);
    }

    public IEnumerator<Error> GetEnumerator()
    {
        if (Count == 1)
        {
            yield return _one;
            yield break;
        }

        if (Count > 1)
        {
            for (var i = 0; i < _many!.Length; i++)
            {
                yield return _many[i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
