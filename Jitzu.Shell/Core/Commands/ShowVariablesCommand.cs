using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Shows global variables in the session.
/// </summary>
public class ShowVariablesCommand : CommandBase
{
    public ShowVariablesCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Markup.FromString("[bold]Global Variables:[\\]"));
        sb.AppendLine();

        var globals = Session.Program.Globals
            .OrderBy(g => g.Key)
            .ToList();

        if (globals.Count == 0)
        {
            sb.AppendLine("  (none defined)");
        }
        else
        {
            foreach (var (name, type) in globals)
            {
                var typeName = type.Name;
                sb.AppendLine(Markup.FromString($@"  [fg:LightBlue_1]{name}[\] : [fg:green]{typeName}[\]"));
            }
        }

        return Task.FromResult(new ShellResult(ResultType.Jitzu, sb.ToString(), null));
    }
}
