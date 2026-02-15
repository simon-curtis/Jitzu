using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays command history.
/// </summary>
public class HistoryCommand : CommandBase
{
    public HistoryCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (HistoryManager == null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("History not available")));

        var sb = new StringBuilder();
        var count = HistoryManager.Count;
        var gutterWidth = count.ToString().Length;
        var dimColor = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;

        for (var i = 0; i < count; i++)
        {
            var lineNum = (i + 1).ToString().PadLeft(gutterWidth);
            sb.AppendLine($"{dimColor}{lineNum}{reset}  {new string(HistoryManager[i].ToArray())}");
        }

        return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString(), null));
    }
}
