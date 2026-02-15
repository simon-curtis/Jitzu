using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Recursively searches for files and directories.
/// </summary>
public class FindCommand : CommandBase
{
    public FindCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "",
                new Exception("Usage: find <path> [-name pattern] [-type f|d] [-ext .cs]")));

        try
        {
            string? searchPath = null;
            string? namePattern = null;
            string? extension = null;
            char? typeFilter = null; // 'f' for file, 'd' for directory

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                switch (arg)
                {
                    case "-name" when i + 1 < args.Length:
                        namePattern = args.Span[++i];
                        break;
                    case "-type" when i + 1 < args.Length:
                        typeFilter = args.Span[++i][0];
                        break;
                    case "-ext" when i + 1 < args.Length:
                        extension = args.Span[++i];
                        if (!extension.StartsWith('.')) extension = "." + extension;
                        break;
                    default:
                        searchPath ??= arg;
                        break;
                }
            }

            searchPath ??= ".";
            var fullPath = ExpandPath(searchPath);

            if (!Directory.Exists(fullPath))
                return Task.FromResult(new ShellResult(ResultType.Error, "",
                    new Exception($"No such directory: {searchPath}")));

            var sb = new StringBuilder();
            var dirColor = Theme["ls.directory"];
            var reset = ThemeConfig.Reset;
            var count = 0;

            foreach (var entry in Directory.EnumerateFileSystemEntries(fullPath, "*", SearchOption.AllDirectories))
            {
                var isDir = Directory.Exists(entry);
                var name = Path.GetFileName(entry);

                // Type filter
                if (typeFilter == 'f' && isDir) continue;
                if (typeFilter == 'd' && !isDir) continue;

                // Name pattern (supports * and ?)
                if (namePattern != null)
                {
                    var pattern = namePattern.Replace("*", ".*").Replace("?", ".");
                    if (!System.Text.RegularExpressions.Regex.IsMatch(name, $"^{pattern}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        continue;
                }

                // Extension filter
                if (extension != null && !name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    continue;

                var relative = Path.GetRelativePath(Environment.CurrentDirectory, entry);
                if (isDir)
                    sb.AppendLine($"{dirColor}{relative}/{reset}");
                else
                    sb.AppendLine(relative);

                count++;
                if (count >= 1000)
                {
                    sb.AppendLine($"{ThemeConfig.Dim}... truncated at 1000 results{reset}");
                    break;
                }
            }

            if (count == 0)
                return Task.FromResult(new ShellResult(ResultType.OsCommand, "No matches found.", null));

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
