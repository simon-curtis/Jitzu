using System.Diagnostics.CodeAnalysis;

namespace Jitzu.Core.Types;

public interface IUnion
{
    object? Value { get; }
    string Format();
}

public interface IUnion<TUnion> : IUnion where TUnion : IUnion<TUnion>
{
    // Creates a union from a value
    static abstract bool TryCreate(object? value, [NotNullWhen(true)] out TUnion union);
}