using System.Collections.Immutable;

namespace Jitzu.Core;

public ref struct ReadOnlySpanSelectStatefulEnumerable<T, TState, TOut>(
    ReadOnlySpan<T> input,
    TState state,
    Func<TState, T, TOut> map)
{
    private int _index = 0;
    private readonly ReadOnlySpan<T> _input = input;
    public TOut Current { get; private set; } = default!;

    public bool MoveNext()
    {
        if (_index > _input.Length - 1)
            return false;

        Current = map(state, _input[_index++]);
        return true;
    }

    public TOut[] ToArray()
    {
        Span<TOut> items = new TOut[_input.Length - _index];

        var index = 0;
        while (MoveNext())
            items[index++] = Current;

        return items.ToArray();
    }

    public ImmutableArray<TOut> ToImmutableArray()
    {
        var builder = ImmutableArray.CreateBuilder<TOut>();

        while (MoveNext())
            builder.Add(Current);

        return builder.ToImmutable();
    }

    public bool All()
    {
        while (MoveNext())
            if (Current is false)
                return false;

        return true;
    }

    public TOut Aggregate()
    {
        while (MoveNext())
        {
        }

        return Current;
    }
}