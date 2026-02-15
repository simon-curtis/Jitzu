using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Prints files with lines in reverse order (opposite of cat).
/// </summary>
public class TacCommand : CommandBase
{
    public TacCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: tac <file> [file2 ...]"));

        try
        {
            var sb = new StringBuilder();

            var argArray = args.ToArray();
            foreach (var arg in argArray)
            {
                var path = ExpandPath(arg);
                if (!File.Exists(path))
                {
                    sb.AppendLine($"tac: {arg}: No such file");
                    continue;
                }

                var lines = await File.ReadAllLinesAsync(path);
                for (var i = lines.Length - 1; i >= 0; i--)
                    sb.AppendLine(lines[i]);
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
