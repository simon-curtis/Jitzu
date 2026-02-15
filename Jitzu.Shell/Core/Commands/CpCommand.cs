namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Copies files or directories.
/// </summary>
public class CpCommand : CommandBase
{
    public CpCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: cp [-r] <source> <destination>")));

        try
        {
            var recursive = false;
            var paths = new List<string>();

            foreach (var arg in args.Span)
            {
                if (arg is "-r" or "--recursive")
                    recursive = true;
                else
                    paths.Add(arg);
            }

            if (paths.Count < 2)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: cp [-r] <source> <destination>")));

            var src = ExpandPath(paths[0]);
            var dst = ExpandPath(paths[1]);

            if (Directory.Exists(src))
            {
                if (!recursive)
                    return Task.FromResult(new ShellResult(ResultType.Error, "",
                        new Exception($"'{paths[0]}' is a directory (use -r to copy)")));
                CopyDirectory(src, dst);
            }
            else if (File.Exists(src))
            {
                if (Directory.Exists(dst))
                    dst = Path.Combine(dst, Path.GetFileName(src));
                File.Copy(src, dst, overwrite: true);
            }
            else
            {
                return Task.FromResult(new ShellResult(ResultType.Error, "",
                    new Exception($"No such file or directory: {paths[0]}")));
            }

            return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
