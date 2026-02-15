namespace Jitzu.Core.Runtime;

public interface IShellFunction;

public record SystemFunction(string Name) : IShellFunction;
