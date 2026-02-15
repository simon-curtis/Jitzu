namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Creates temporary files or directories.
/// </summary>
public class MktempCommand : CommandBase
{
    public MktempCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        try
        {
            var isDir = false;
            string? suffix = null;
            string? prefix = "tmp.";

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                switch (arg)
                {
                    case "-d" or "--directory":
                        isDir = true;
                        break;
                    case "--suffix" when i + 1 < args.Length:
                        suffix = args.Span[++i];
                        break;
                    case "-p" when i + 1 < args.Length:
                        prefix = args.Span[++i];
                        break;
                    default:
                        // Treat as a template: XXXXXX gets replaced with random chars
                        prefix = arg.Replace("XXXXXX", "").Replace("XXXX", "");
                        break;
                }
            }

            var name = $"{prefix}{Path.GetRandomFileName()}{suffix}";
            var fullPath = Path.Combine(Path.GetTempPath(), name);

            if (isDir)
            {
                Directory.CreateDirectory(fullPath);
            }
            else
            {
                File.Create(fullPath).Dispose();
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, fullPath, null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
