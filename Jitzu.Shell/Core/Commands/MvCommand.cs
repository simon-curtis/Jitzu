namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Moves or renames files and directories.
/// </summary>
public class MvCommand : CommandBase
{
    public MvCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: mv <source> <destination>")));

        try
        {
            var src = ExpandPath(args.Span[0]);
            var dst = ExpandPath(args.Span[1]);

            // If destination is an existing directory, move into it
            if (Directory.Exists(dst))
                dst = Path.Combine(dst, Path.GetFileName(src));

            if (Directory.Exists(src))
                Directory.Move(src, dst);
            else if (File.Exists(src))
                File.Move(src, dst);
            else
                return Task.FromResult(new ShellResult(ResultType.Error, "",
                    new Exception($"No such file or directory: {args.Span[0]}")));

            return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
