using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Changes file attributes (Windows: ReadOnly, Hidden, System).
/// </summary>
public class ChmodCommand : CommandBase
{
    public ChmodCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return Task.FromResult(new ShellResult(ResultType.Error, "",
                new Exception("Usage: chmod <+/-><r|h|s> <file> [file2 ...]\n  +r/-r  Toggle ReadOnly\n  +h/-h  Toggle Hidden\n  +s/-s  Toggle System")));

        try
        {
            var mode = args.Span[0];
            if (mode.Length != 2 || (mode[0] != '+' && mode[0] != '-'))
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Invalid mode: {mode}. Use +r, -r, +h, -h, +s, -s")));

            var add = mode[0] == '+';
            var flag = char.ToLower(mode[1]) switch
            {
                'r' => FileAttributes.ReadOnly,
                'h' => FileAttributes.Hidden,
                's' => FileAttributes.System,
                _ => (FileAttributes?)null
            };

            if (flag == null)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Unknown attribute: {mode[1]}. Use r (ReadOnly), h (Hidden), s (System)")));

            var sb = new StringBuilder();
            var reset = ThemeConfig.Reset;

            for (var i = 1; i < args.Length; i++)
            {
                var filePath = args.Span[i];
                var path = ExpandPath(filePath);

                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    sb.AppendLine($"chmod: cannot access '{filePath}': No such file or directory");
                    continue;
                }

                var currentAttrs = File.GetAttributes(path);
                var newAttrs = add
                    ? currentAttrs | flag.Value
                    : currentAttrs & ~flag.Value;
                File.SetAttributes(path, newAttrs);

                var attrList = new List<string>();
                if (newAttrs.HasFlag(FileAttributes.ReadOnly)) attrList.Add("ReadOnly");
                if (newAttrs.HasFlag(FileAttributes.Hidden)) attrList.Add("Hidden");
                if (newAttrs.HasFlag(FileAttributes.System)) attrList.Add("System");

                sb.AppendLine($"{filePath}: {ThemeConfig.Dim}{(attrList.Count > 0 ? string.Join(", ", attrList) : "None")}{reset}");
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
