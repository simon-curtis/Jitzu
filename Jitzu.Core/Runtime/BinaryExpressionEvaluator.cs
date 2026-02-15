using System.Runtime.CompilerServices;
using Jitzu.Core.Logging;

namespace Jitzu.Core.Runtime;

public static class BinaryExpressionEvaluator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Equal(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromBool(a.I32 == b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromBool(Math.Abs(a.I32 - b.F64) < 0),
        (ValueKind.Double, ValueKind.Int) => Value.FromBool(Math.Abs(a.F64 - b.I32) < 0),
        (ValueKind.Double, ValueKind.Double) => Value.FromBool(Math.Abs(a.F64 - b.F64) < 0),
        _ => Throw("lt", a, b)
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Add(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 + b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromDouble(a.I32 + b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromDouble(a.F64 + b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromDouble(a.F64 + b.F64),
        _ => Throw("add", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Sub(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 - b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromDouble(a.I32 - b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromDouble(a.F64 - b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromDouble(a.F64 - b.F64),
        _ => Throw("sub", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Mul(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 * b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromDouble(a.I32 * b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromDouble(a.F64 * b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromDouble(a.F64 * b.F64),
        _ => Throw("mul", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Div(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 / b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromDouble(a.I32 / b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromDouble(a.F64 / b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromDouble(a.F64 / b.F64),
        _ => Throw("div", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value Mod(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 % b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromDouble(a.I32 % b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromDouble(a.F64 % b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromDouble(a.F64 % b.F64),
        _ => Throw("mod", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value LessThan(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromBool(a.I32 < b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromBool(a.I32 < b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromBool(a.F64 < b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromBool(a.F64 < b.F64),
        _ => Throw("lt", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value LessThanOrEqual(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromBool(a.I32 <= b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromBool(a.I32 <= b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromBool(a.F64 <= b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromBool(a.F64 <= b.F64),
        _ => Throw("lte", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value GreaterThan(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromBool(a.I32 > b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromBool(a.I32 > b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromBool(a.F64 > b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromBool(a.F64 > b.F64),
        _ => Throw("gt", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value GreaterThanOrEqual(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromBool(a.I32 >= b.I32),
        (ValueKind.Int, ValueKind.Double) => Value.FromBool(a.I32 >= b.F64),
        (ValueKind.Double, ValueKind.Int) => Value.FromBool(a.F64 >= b.I32),
        (ValueKind.Double, ValueKind.Double) => Value.FromBool(a.F64 >= b.F64),
        _ => Throw("gte", a, b)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value BitwiseOr(Value a, Value b) => (a.Kind, b.Kind) switch
    {
        (ValueKind.Int, ValueKind.Int) => Value.FromInt(a.I32 | b.I32),
        _ => Throw("bitwise_or", a, b)
    };

    // Helper to keep exception instantiation out of the mainline JIT path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Value Throw(string op, Value a, Value b)
    {
        throw new OperationNotSupportedException(op, a, b);
    }
}

public class OperationNotSupportedException(string op, Value left, Value? right) : Exception(
    $"Operation {op} not supported for types '{ValueFormatter.Format(left)}' and '{ValueFormatter.Format(right)}'");