namespace Jitzu.Core.Runtime;

public class UserFunction(string name, Chunk chunk) : IShellFunction
{
    public Type? ParentType { get; init; }
    public int LocalCount { get; init; }
    public UserFunctionParameter[] Parameters { get; set; } = [];
    public Type? FunctionReturnType { get; set; }
    public Chunk Chunk { get; set; } = chunk;

    public override string ToString()
    {
        var sb = ObjectPools.StringBuilderPool.Rent();
        try
        {
            if (ParentType is not null)
                sb.Append($"{ParentType.FullName}.");

            sb.Append(name);
            sb.Append('(');
            sb.Append(Parameters.Select(p => $"{p.Name}: {p.Type.Name}").Join(", "));
            sb.Append(')');

            if (FunctionReturnType is not null)
                sb.Append($": {FunctionReturnType.FullName}");

            return sb.ToString();
        }
        finally
        {
            ObjectPools.StringBuilderPool.Return(sb);
        }
    }
}