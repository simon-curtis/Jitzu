namespace Jitzu.Core.Runtime;

public class Closure(UserFunction function, UpvalueCell[] upvalues) : IShellFunction
{
    public UserFunction Function { get; } = function;
    public UpvalueCell[] Upvalues { get; } = upvalues;

    public override string ToString() => $"<closure {Function}>";
}
