using System.Runtime.CompilerServices;

namespace Jitzu.Core.Runtime;

public class ProgramStack(int capacity = 256)
{
    private readonly Value[] _globals = new Value[64];
    internal Value[] Stack = GC.AllocateUninitializedArray<Value>(capacity);
    internal int StackPointer = -1;
    internal int FrameBase; // Add this to track current frame base

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(Value value) => Stack[++StackPointer] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push<T>(T value) where T : class => Stack[++StackPointer] = value switch
    {
        int i => Value.FromInt(i),
        double d => Value.FromDouble(d),
        bool b => Value.FromBool(b),
        _ => Value.FromRef(value)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(bool value) => Stack[++StackPointer] = Value.FromBool(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(int value) => Stack[++StackPointer] = Value.FromInt(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(double value) => Stack[++StackPointer] = Value.FromDouble(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Swap(Value value) => Stack[StackPointer] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Swap<T>(T value) where T : class => Stack[StackPointer] = Value.FromRef(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Swap(int value) => Stack[StackPointer] = Value.FromInt(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Swap(double value) => Stack[StackPointer] = Value.FromDouble(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Value Peek(int offset = 0) => ref Stack[StackPointer - offset];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Value Pop() => Stack[StackPointer--];

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void PushFrame(int localCount)
    {
        if (StackPointer + 1 + localCount > Stack.Length)
            Array.Resize(ref Stack, Stack.Length * 2);

        FrameBase = StackPointer + 1; // Frame starts after current stack top
        StackPointer += localCount; // Reserve space for locals
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetPointer(int stackPointer)
    {
        StackPointer = stackPointer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetFrameBase(int frameBase)
    {
        FrameBase = frameBase;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Value GetGlobal(int slotIndex) => _globals[slotIndex];

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetGlobal(int slotIndex, Value value) => _globals[slotIndex] = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Value GetLocal(int slotIndex) => Stack[FrameBase + slotIndex];

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetLocal(int slotIndex, Value value) => Stack[FrameBase + slotIndex] = value;
}