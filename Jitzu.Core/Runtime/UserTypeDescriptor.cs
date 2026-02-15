namespace Jitzu.Core.Runtime;

public sealed class UserTypeDescriptor
{
    public required string Name { get; init; }
    public string FullName { get; set; } = "";  // Fully qualified name (e.g., "namespace.TypeName")
    public required UserFieldDescriptor[] Fields { get; init; }
    public Type? CreatedType { get; set; }
}

public sealed class UserFieldDescriptor
{
    public required string Name { get; init; }
    public required Type ClrType { get; init; }
    public bool IsPublic { get; init; }
    public bool IsMutable { get; init; } // if false => init-only, else settable
    public object? DefaultValue { get; init; }
}