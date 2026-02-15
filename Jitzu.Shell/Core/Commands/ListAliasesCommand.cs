using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Lists all defined command aliases.
/// </summary>
public class ListAliasesCommand : CommandBase
{
    public ListAliasesCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (AliasManager == null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Alias manager not available")));

        var aliases = AliasManager.Aliases;
        if (aliases.Count == 0)
            return Task.FromResult(new ShellResult(ResultType.Jitzu, "No aliases defined.", null));

        var sb = new StringBuilder();
        foreach (var (name, value) in aliases.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine($"  {name}='{value}'");

        return Task.FromResult(new ShellResult(ResultType.Jitzu, sb.ToString(), null));
    }
}
