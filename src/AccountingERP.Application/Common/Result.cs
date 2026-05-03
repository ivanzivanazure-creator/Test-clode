namespace AccountingERP.Application.Common;

/// <summary>
/// Discriminated union representing either a successful value or an error string.
/// Avoids using exceptions for control flow in the application layer.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _error;

    private Result(T value)
    {
        _value    = value;
        _error    = null;
        IsSuccess = true;
    }

    private Result(string error)
    {
        _value    = default;
        _error    = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value of a failed Result. Error: {_error}");

    public string Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error of a successful Result.");

    // ── Factory methods ───────────────────────────────────────────────────────

    public static Result<T> Success(T value)          => new(value);
    public static Result<T> Failure(string error)     => new(error);

    // ── Implicit conversions ──────────────────────────────────────────────────

    public static implicit operator Result<T>(T value)      => Success(value);
    public static implicit operator Result<T>(string error)  => Failure(error);

    // ── Functional helpers ────────────────────────────────────────────────────

    /// <summary>Maps the value if successful; otherwise propagates the error.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
        => IsSuccess ? Result<TOut>.Success(map(_value!)) : Result<TOut>.Failure(_error!);

    /// <summary>Chains a fallible operation if the current result is successful.</summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
        => IsSuccess ? bind(_value!) : Result<TOut>.Failure(_error!);

    public override string ToString()
        => IsSuccess ? $"Success({_value})" : $"Failure({_error})";
}
