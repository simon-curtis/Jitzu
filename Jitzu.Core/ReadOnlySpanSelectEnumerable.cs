namespace Jitzu.Core;

public ref struct ReadOnlySpanSelectEnumerable<T, TOut>(ReadOnlySpan<T> input, Func<T, TOut> map)
{
    private int _index = 0;
    private readonly ReadOnlySpan<T> _input = input;
    public TOut Current { get; private set; } = default!;

    public bool MoveNext()
    {
        if (_index > _input.Length - 1)
            return false;

        Current = map(_input[_index++]);
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
    
    public List<TOut> ToList()
    {
        var items = new List<TOut>(_input.Length - _index);

        while (MoveNext())
            items.Add(Current);

        return items;
    }

    public bool All()
    {
        while (MoveNext())
            if (Current is false)
                return false;

        return true;
    }
}