using Jitzu.Shell.Infrastructure.Update;

namespace Jitzu.Shell.Core.Commands;

public class UpgradeCommand : CommandBase
{
    public UpgradeCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var force = args.Span.Length > 0 && args.Span[0] is "--force" or "-f";

        try
        {
            await SelfUpdater.RunAsync(force);
            return new ShellResult(ResultType.Jitzu, null, null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, null, ex);
        }
    }
}
