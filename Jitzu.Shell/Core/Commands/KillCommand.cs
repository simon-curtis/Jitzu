using System.Diagnostics;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Kills processes by PID or job ID.
/// </summary>
public class KillCommand : CommandBase
{
    public KillCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: kill [-9] <pid|%jobid>")));

        try
        {
            var forceKill = false;
            int? targetPid = null;
            int? jobId = null;

            foreach (var arg in args.Span)
            {
                if (arg is "-9" or "-KILL" or "-kill")
                    forceKill = true;
                else if (arg.StartsWith('%'))
                {
                    if (int.TryParse(arg.AsSpan(1), out var jid))
                        jobId = jid;
                    else
                        return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Invalid job ID: {arg}")));
                }
                else if (int.TryParse(arg, out var pid))
                    targetPid = pid;
                else
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Invalid argument: {arg}")));
            }

            if (jobId.HasValue)
            {
                if (Strategy == null)
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("kill: execution strategy not available")));

                var job = Strategy.Jobs.FirstOrDefault(j => j.Id == jobId.Value);
                if (job == null)
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"kill: no such job %{jobId.Value}")));

                try
                {
                    if (!job.Process.HasExited)
                    {
                        if (forceKill)
                            job.Process.Kill(entireProcessTree: true);
                        else
                            job.Process.Kill();
                    }

                    return Task.FromResult(new ShellResult(ResultType.OsCommand, $"Killed job %{jobId.Value} (pid {job.Process.Id})", null));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"kill: failed to kill job %{jobId.Value}: {ex.Message}")));
                }
            }

            if (!targetPid.HasValue)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: kill [-9] <pid|%jobid>")));

            try
            {
                var process = Process.GetProcessById(targetPid.Value);
                if (forceKill)
                    process.Kill(entireProcessTree: true);
                else if (!process.CloseMainWindow())
                    process.Kill();

                return Task.FromResult(new ShellResult(ResultType.OsCommand, $"Killed process {targetPid.Value}", null));
            }
            catch (ArgumentException)
            {
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"kill: no process with pid {targetPid.Value}")));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"kill: {ex.Message}")));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
