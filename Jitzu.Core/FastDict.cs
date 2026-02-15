using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Standart.Hash.xxHash;

namespace Jitzu.Core;

public class FastDict<TValue>()
{
    public static FastDict<TValue> Empty => new();

    public readonly Dictionary<ulong, TValue> Values = [];

    public bool ContainsKey(ulong key) => Values.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue? GetValueOrDefault(ReadOnlySpan<char> key) => Values.GetValueOrDefault(FastDict.ComputeHash(key));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue? GetValueOrDefault(ulong key) => Values.GetValueOrDefault(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ReadOnlySpan<char> key, [NotNullWhen(true)] out TValue? variable)
    {
        Values.TryGetValue(FastDict.ComputeHash(key), out variable);
        return variable is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(ulong key, [NotNullWhen(true)] out TValue? variable)
    {
        var found = Values.TryGetValue(key, out variable);
        return found && variable is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<char> key, TValue value)
    {
        Values[FastDict.ComputeHash(key)] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ulong key, TValue value)
    {
        Values[key] = value;
    }

    public TValue this[ReadOnlySpan<char> key]
    {
        get => Values[FastDict.ComputeHash(key)];
        init => Add(key, value);
    }

    public TValue this[ulong key]
    {
        get => Values[key];
        set => Values[key] = value;
    }

    public void Clear() => Values.Clear();

    public FrozenFastDict<TValue> ToFrozen()
    {
        return new FrozenFastDict<TValue>(Values.ToFrozenDictionary());
    }

    public static FastDict<TValue> FromDictionary(Dictionary<string, TValue> values)
    {
        var dict = new FastDict<TValue>();
        foreach (var item in values)
            dict.Add(item.Key, item.Value);
        return dict;
    }
}

public static class FastDict
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<char> span)
    {
        return xxHash32.ComputeHash(MemoryMarshal.AsBytes(span), span.Length);
    }
}