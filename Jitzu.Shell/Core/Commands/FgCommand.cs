namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Brings a background job to the foreground.
/// </summary>
public class FgCommand : CommandBase
{
    public FgCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (Strategy == null)
            return new ShellResult(ResultType.Error, "", new Exception("fg: not available"));

        int? jobId = null;
        if (args.Length > 0)
        {
            var arg = args.Span[0].TrimStart('%');
            if (int.TryParse(arg, out var id))
                jobId = id;
        }

        return await Strategy.ForegroundJobAsync(jobId);
    }
}
