using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Shows available types in the session.
/// </summary>
public class ShowTypesCommand : CommandBase
{
    public ShowTypesCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Markup.FromString("[bold]Available Types:[\\]"));
        sb.AppendLine();

        // Group types by category
        var builtinTypes = new List<string>();
        var userTypes = new List<string>();
        var importedTypes = new List<string>();

        foreach (var (name, type) in Session.Program.SimpleTypeCache.OrderBy(t => t.Key))
        {
            // Categorize types
            if (type.Namespace?.StartsWith("Jitzu.") == true || type.Namespace == null)
                builtinTypes.Add(name);
            else if (type.Assembly.IsDynamic)
                userTypes.Add(name);
            else
                importedTypes.Add(name);
        }

        if (builtinTypes.Count > 0)
        {
            sb.AppendLine(Markup.FromString("[fg:yellow]Built-in Types:[\\]"));
            foreach (var typeName in builtinTypes)
                sb.AppendLine($"  {typeName}");
            sb.AppendLine();
        }

        if (userTypes.Count > 0)
        {
            sb.AppendLine(Markup.FromString("[fg:cyan]User-Defined Types:[\\]"));
            foreach (var typeName in userTypes)
                sb.AppendLine($"  {typeName}");
            sb.AppendLine();
        }

        if (importedTypes.Count > 0)
        {
            sb.AppendLine(Markup.FromString("[fg:green]Imported Types:[\\]"));
            foreach (var typeName in importedTypes)
                sb.AppendLine($"  {typeName}");
        }

        if (builtinTypes.Count == 0 && userTypes.Count == 0 && importedTypes.Count == 0)
        {
            sb.AppendLine("  (no types available)");
        }

        return Task.FromResult(new ShellResult(ResultType.Jitzu, sb.ToString(), null));
    }
}
