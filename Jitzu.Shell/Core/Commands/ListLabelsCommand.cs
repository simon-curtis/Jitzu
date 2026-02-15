using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Lists all defined path labels.
/// </summary>
public class ListLabelsCommand : CommandBase
{
    public ListLabelsCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (LabelManager is null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Label manager not available")));

        var labels = LabelManager.Labels;
        if (labels.Count == 0)
            return Task.FromResult(new ShellResult(ResultType.Jitzu, "No labels defined.", null));

        var sb = new StringBuilder();
        foreach (var (name, path) in labels.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine($"  {name}: → {path}");

        return Task.FromResult(new ShellResult(ResultType.Jitzu, sb.ToString(), null));
    }
}
