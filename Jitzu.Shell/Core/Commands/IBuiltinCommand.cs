namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Interface for built-in shell commands.
/// </summary>
public interface IBuiltinCommand
{
    /// <summary>
    /// Execute the command with the given arguments.
    /// </summary>
    /// <param name="args">Command arguments (excluding the command name itself)</param>
    /// <returns>Result of command execution</returns>
    Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args);
}
