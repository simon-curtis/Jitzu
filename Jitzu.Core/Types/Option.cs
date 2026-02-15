using Jitzu.Core.Logging;

namespace Jitzu.Core.Types;

public record struct Option<TSome> : IUnion<Option<TSome>>, ICanFallback, ITruthy
{
    public Option(Some<TSome> some)
    {
        Value = some;
        IsTruthy = true;
    }

    public Option(None none)
    {
        Value = none;
        IsTruthy = false;
    }

    public bool IsTruthy { get; }
    public object? Value { get; }

    public static bool TryCreate(object? value, out Option<TSome> union)
    {
        (var result, union) = value switch
        {
            Some<TSome> v => (true, new Option<TSome>(v)),
            None v => (true, new Option<TSome>(v)),
            _ => (false, default),
        };
        return result;
    }

    public string Format()
    {
        return Value switch
        {
            Some<TSome>(var val) => $"Some({ValueFormatter.Format(val)})",
            None => "None",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public object Fallback(object fallbackValue)
    {
        return Value is Some<TSome>(var val) ? val! : fallbackValue;
    }

    public object Unwrap()
    {
        return Value switch
        {
            Some<TSome>(var obj) => obj!,
            None => throw new Exception("Value was none"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override string ToString()
    {
        return Value switch
        {
            Some<TSome>(var val) => $"Some({val?.ToString()})",
            None => "None",
            _ => throw new Exception("Unreachable")
        };
    }
}

public record Some<TSome>(TSome Value);

public sealed record None()
{
    public static readonly None Instance = new();
}