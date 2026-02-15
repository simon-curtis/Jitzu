namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Removes files or directories.
/// </summary>
public class RmCommand : CommandBase
{
    public RmCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: rm [-r] <path> [path2 ...]")));

        try
        {
            var recursive = false;
            var paths = new List<string>();

            foreach (var arg in args.Span)
            {
                if (arg is "-r" or "-rf" or "--recursive")
                    recursive = true;
                else
                    paths.Add(arg);
            }

            if (paths.Count == 0)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("No path specified")));

            foreach (var p in paths)
            {
                var path = ExpandPath(p);

                if (Directory.Exists(path))
                {
                    if (!recursive)
                        return Task.FromResult(new ShellResult(ResultType.Error, "",
                            new Exception($"'{p}' is a directory (use -r to remove)")));
                    Directory.Delete(path, true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    return Task.FromResult(new ShellResult(ResultType.Error, "",
                        new Exception($"No such file or directory: {p}")));
                }
            }

            return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
