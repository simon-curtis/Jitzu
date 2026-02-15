namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Pauses execution for a specified number of seconds.
/// </summary>
public class SleepCommand : CommandBase
{
    public SleepCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: sleep <seconds>"));

        if (!double.TryParse(args.Span[0], out var seconds) || seconds < 0)
            return new ShellResult(ResultType.Error, "", new Exception($"Invalid duration: {args.Span[0]}"));

        await Task.Delay(TimeSpan.FromSeconds(seconds));
        return new ShellResult(ResultType.Jitzu, "", null);
    }
}
