using System.Diagnostics;

namespace Jitzu.Shell.Core;

/// <summary>
/// Extension methods for <see cref="Process"/> used across the shell.
/// </summary>
internal static class ProcessExtensions
{
    /// <summary>
    /// Waits for a child process to exit while suppressing Ctrl+C on the shell.
    /// Ctrl+C is delivered to the entire console process group (on both Windows and Unix),
    /// which would also terminate the shell. Setting <c>args.Cancel = true</c> prevents the
    /// CLR from raising the signal as an exception in this process while the child handles it.
    /// </summary>
    private static readonly ConsoleCancelEventHandler SuppressCancelHandler = (_, args) => args.Cancel = true;

    public static async Task WaitForExitSuppressingCancelAsync(this Process process, CancellationToken cancellationToken = default)
    {
        Console.CancelKeyPress += SuppressCancelHandler;
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            Console.CancelKeyPress -= SuppressCancelHandler;
        }
    }
}
