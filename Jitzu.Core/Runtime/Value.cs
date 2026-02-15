using System.Runtime.CompilerServices;

namespace Jitzu.Core.Runtime;

public enum ValueKind : byte
{
    Null,
    Int,
    Double,
    Bool,
    Ref,
}

public readonly record struct Value
{
    public readonly ValueKind Kind;
    public readonly int I32;
    public readonly double F64;
    public readonly bool B;
    public readonly object Ref;

    private Value(ValueKind kind, int i = 0, double d = 0, bool b = false, object? o = null)
    {
        Kind = kind;
        I32 = i;
        F64 = d;
        B = b;
        Ref = o!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value FromInt(int v) => new(ValueKind.Int, i: v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value FromDouble(double v) => new(ValueKind.Double, d: v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value FromBool(bool v) => new(ValueKind.Bool, b: v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value FromRef(object? v) =>
        new(v == null ? ValueKind.Null : ValueKind.Ref, o: v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object AsObject() =>
        Kind switch
        {
            ValueKind.Int => I32,
            ValueKind.Double => F64,
            ValueKind.Bool => B,
            ValueKind.Ref => Ref,
            _ => throw new Exception("Invalid state")
        };

    public override string ToString()
    {
        return $"Value::{Kind}({AsObject().ToString()})";
    }
}