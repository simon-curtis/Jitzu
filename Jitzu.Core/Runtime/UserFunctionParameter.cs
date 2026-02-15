namespace Jitzu.Core.Runtime;

public record struct UserFunctionParameter(string Name, Type Type, bool IsSelf = false);