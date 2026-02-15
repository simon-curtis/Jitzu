using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jitzu.Core;

public class FrozenFastDict<TValue>()
{
    private readonly FrozenDictionary<ulong, TValue> _values = FrozenDictionary<ulong, TValue>.Empty;
    public int Count => _values.Count;

    public FrozenFastDict(IDictionary<string, TValue> original) : this()
    {
        var values = new Dictionary<ulong, TValue>();
        foreach (var (originalKey, value) in original)
            values[FastDict.ComputeHash(originalKey)] = value;
        _values = values.ToFrozenDictionary();
    }

    public FrozenFastDict(FrozenDictionary<ulong, TValue> frozenDictionary) : this()
    {
        _values = frozenDictionary;
    }

    public ImmutableArray<TValue> Values => _values.Values;

    public bool ContainsKey(ulong key) => _values.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ReadOnlySpan<char> key, [NotNullWhen(true)] out TValue? variable)
    {
        _values.TryGetValue(FastDict.ComputeHash(key), out variable);
        return variable is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ulong key, [NotNullWhen(true)] out TValue? variable)
    {
        _values.TryGetValue(key, out variable);
        return variable is not null;
    }

    public TValue this[string key] => _values[FastDict.ComputeHash(key)];
    public TValue this[ulong key] => _values[key];
}