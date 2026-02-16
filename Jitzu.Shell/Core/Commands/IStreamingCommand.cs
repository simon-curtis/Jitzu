namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Interface for commands that can produce streaming output.
/// This allows commands to avoid buffering large outputs in memory.
/// </summary>
public interface IStreamingCommand
{
    /// <summary>
    /// Executes the command and returns a stream of output lines.
    /// </summary>
    IAsyncEnumerable<string> StreamAsync(ReadOnlyMemory<string> args, CancellationToken cancellationToken = default);
}
