using System.Runtime.InteropServices;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Creates hard or symbolic links.
/// </summary>
public class LnCommand : CommandBase
{
    public LnCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: ln [-s] <target> <link>")));

        try
        {
            var symbolic = false;
            var paths = new List<string>();

            foreach (var arg in args.Span)
            {
                if (arg is "-s" or "--symbolic")
                    symbolic = true;
                else
                    paths.Add(arg);
            }

            if (paths.Count < 2)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: ln [-s] <target> <link>")));

            var target = ExpandPath(paths[0]);
            var linkPath = ExpandPath(paths[1]);

            if (File.Exists(linkPath) || Directory.Exists(linkPath))
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Link already exists: {paths[1]}")));

            if (!File.Exists(target) && !Directory.Exists(target))
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Target not found: {paths[0]}")));

            var isDirectory = Directory.Exists(target);

            if (symbolic)
            {
                if (isDirectory)
                    Directory.CreateSymbolicLink(linkPath, target);
                else
                    File.CreateSymbolicLink(linkPath, target);
            }
            else
            {
                if (isDirectory)
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Hard links to directories are not supported")));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    CreateHardLink(linkPath, target, IntPtr.Zero);
                else
                    File.CreateSymbolicLink(linkPath, target); // fallback to symlink on non-Windows
            }

            var linkType = symbolic ? "symbolic" : "hard";
            return Task.FromResult(new ShellResult(ResultType.OsCommand, $"Created {linkType} link: {paths[1]} -> {paths[0]}", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }

    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
}
