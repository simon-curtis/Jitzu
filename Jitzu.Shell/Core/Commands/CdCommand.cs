namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Changes the current working directory.
/// </summary>
public class CdCommand : CommandBase
{
    private string? _previousDirectory;

    public CdCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        try
        {
            var targetDir = args.Length > 0 ? args.Span[0] : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // cd - goes to previous directory
            if (targetDir is "-")
            {
                if (_previousDirectory == null)
                    return Task.FromResult(new ShellResult(ResultType.Error, "",
                        new Exception("No previous directory")));
                targetDir = _previousDirectory;
            }

            // Expand labels and ~ to full paths
            targetDir = ExpandPath(targetDir);

            // Validate directory exists
            if (!Directory.Exists(targetDir))
                return Task.FromResult(new ShellResult(ResultType.Error, "",
                    new Exception($"Directory not found: {targetDir}")));

            // Save current directory before changing
            var currentDir = Directory.GetCurrentDirectory();

            // Change directory
            Directory.SetCurrentDirectory(targetDir);
            _previousDirectory = currentDir;

            return Task.FromResult(new ShellResult(ResultType.OsCommand, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
