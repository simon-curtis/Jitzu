namespace Jitzu.Core.Runtime;

internal readonly struct CallFrame(UserFunction userFunction, int ip, int stackPointer, int frameBase)
{
    public UserFunction UserFunction { get; } = userFunction;
    public int IP { get; } = ip;
    public int StackPointer { get; } = stackPointer;
    public int FrameBase { get; } = frameBase;
}