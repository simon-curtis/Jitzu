using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Reverses characters in each line of files.
/// </summary>
public class RevCommand : CommandBase
{
    public RevCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: rev <file> [file2 ...]"));

        try
        {
            var sb = new StringBuilder();

            var argArray = args.ToArray();
            foreach (var arg in argArray)
            {
                var path = ExpandPath(arg);
                if (!File.Exists(path))
                {
                    sb.AppendLine($"rev: {arg}: No such file");
                    continue;
                }

                var lines = await File.ReadAllLinesAsync(path);
                foreach (var line in lines)
                {
                    var chars = line.ToCharArray();
                    Array.Reverse(chars);
                    sb.AppendLine(new string(chars));
                }
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
