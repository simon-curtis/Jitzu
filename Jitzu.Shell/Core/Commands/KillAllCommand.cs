using System.Diagnostics;
using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Kills all processes matching a name.
/// </summary>
public class KillAllCommand : CommandBase
{
    public KillAllCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: killall [-9] <name>")));

        try
        {
            var forceKill = false;
            string? processName = null;

            foreach (var arg in args.Span)
            {
                if (arg is "-9" or "-KILL" or "-kill")
                    forceKill = true;
                else
                    processName = arg;
            }

            if (processName == null)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("No process name specified")));

            var processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"killall: no processes matching '{processName}'")));

            var killed = 0;
            var errors = new List<string>();

            foreach (var proc in processes)
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        if (forceKill)
                            proc.Kill(entireProcessTree: true);
                        else if (!proc.CloseMainWindow())
                            proc.Kill();
                        killed++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"  pid {proc.Id}: {ex.Message}");
                }
                finally
                {
                    proc.Dispose();
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Killed {killed} process{(killed != 1 ? "es" : "")} matching '{processName}'");
            foreach (var err in errors)
                sb.AppendLine(err);

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
