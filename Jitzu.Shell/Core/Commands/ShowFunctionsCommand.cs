using System.Text;
using Jitzu.Core.Runtime;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Shows global functions in the session.
/// </summary>
public class ShowFunctionsCommand : CommandBase
{
    public ShowFunctionsCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Markup.FromString("[bold]Global Functions:[\\]"));
        sb.AppendLine();

        var functions = Session.Program.GlobalFunctions
            .OrderBy(f => f.Key)
            .ToList();

        if (functions.Count == 0)
        {
            sb.AppendLine("  (none defined)");
        }
        else
        {
            foreach (var (name, function) in functions)
            {
                string signature = function switch
                {
                    UserFunction userFunc =>
                        $"{name}({userFunc.Parameters.Length} param{(userFunc.Parameters.Length != 1 ? "s" : "")}) : {userFunc.FunctionReturnType?.Name ?? "void"}",
                    ForeignFunction foreignFunc =>
                        $"{name}({foreignFunc.MethodInfo.GetParameters().Length} param{(foreignFunc.MethodInfo.GetParameters().Length != 1 ? "s" : "")}) : {foreignFunc.MethodInfo.ReturnType.Name}",
                    SystemFunction =>
                        $"{name}(builtin)",
                    _ =>
                        $"{name}(unknown)"
                };

                sb.AppendLine(Markup.FromString($"  [fg:LightBlue_1]{signature}[\\]"));
            }
        }

        return Task.FromResult(new ShellResult(ResultType.Jitzu, sb.ToString(), null));
    }
}
