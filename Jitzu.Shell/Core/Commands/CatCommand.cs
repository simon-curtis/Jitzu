using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays file contents with line numbers.
/// </summary>
public class CatCommand : CommandBase
{
    public CatCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: cat <file>"));

        try
        {
            var filePath = args.Span[0];
            var fullPath = ExpandPath(filePath);

            if (!File.Exists(fullPath))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var lines = await File.ReadAllLinesAsync(fullPath);
            var sb = new StringBuilder();
            var gutterWidth = lines.Length.ToString().Length;
            var dimColor = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            for (var i = 0; i < lines.Length; i++)
            {
                var lineNum = (i + 1).ToString().PadLeft(gutterWidth);
                sb.AppendLine($"{dimColor}{lineNum}{reset}  {lines[i]}");
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
