using Jitzu.Core.Logging;

namespace Jitzu.Core.Types;

public record struct Result<TOk, TErr> : IUnion<Result<TOk, TErr>>, ICanFallback, ICanUnwrap, ITruthy
{
    public Result(Ok<TOk> ok)
    {
        Value = ok;
        IsTruthy = true;
    }

    public Result(Err<TErr> err)
    {
        Value = err;
        IsTruthy = false;
    }

    public bool IsTruthy { get; }
    public object? Value { get; init; }

    public static bool TryCreate(object? value, out Result<TOk, TErr> union)
    {
        (var result, union) = value switch
        {
            Ok<TOk> v => (true, new Result<TOk, TErr>(v)),
            Err<TErr> v => (true, new Result<TOk, TErr>(v)),
            _ => (false, default),
        };
        return result;
    }

    public string Format()
    {
        return Value switch
        {
            Ok<TOk>(var val) => $"Ok({ValueFormatter.Format(val)})",
            Err<TErr>(var err) => $"Err({err})",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object Fallback(object fallbackValue)
    {
        return Value is Ok<TOk>(var val) ? val! : fallbackValue;
    }

    public object Unwrap()
    {
        return Value switch
        {
            Ok<TOk>(var obj) => obj!,
            Err<TErr>(var err) => throw new Exception(ValueFormatter.Format(err)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public sealed record Ok<TOk>(TOk Value)
{
    public override string ToString() => $"Ok({Value!.ToString()})";
}

public sealed record Err<TErr>(TErr Error)
{
    public override string ToString() => $"Err({Error!.ToString()})";
}