namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Creates a new directory.
/// </summary>
public class MkdirCommand : CommandBase
{
    private string? _previousDirectory;

    public MkdirCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: mkdir [-cd] <directory>")));

        try
        {
            var follow = false;
            string? targetDir = null;

            foreach (var arg in args.Span)
            {
                if (arg is "-cd" or "--cd")
                    follow = true;
                else
                    targetDir = arg;
            }

            if (targetDir == null)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: mkdir [-cd] <directory>")));

            var fullPath = ExpandPath(targetDir);

            if (Directory.Exists(fullPath))
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Directory already exists: {fullPath}")));

            Directory.CreateDirectory(fullPath);

            if (follow)
            {
                _previousDirectory = Environment.CurrentDirectory;
                Directory.SetCurrentDirectory(fullPath);
            }

            return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
