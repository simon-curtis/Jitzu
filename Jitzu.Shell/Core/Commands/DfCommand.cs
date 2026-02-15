using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays disk space information for mounted drives.
/// </summary>
public class DfCommand : CommandBase
{
    public DfCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        try
        {
            var sb = new StringBuilder();
            var dim = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;
            var header = $"{"Filesystem",-30} {"Size",8} {"Used",8} {"Avail",8} {"Use%",5}  Mount";
            sb.AppendLine($"{dim}{header}{reset}");

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady) continue;

                var total = drive.TotalSize;
                var free = drive.TotalFreeSpace;
                var used = total - free;
                var usePct = total > 0 ? (int)(used * 100.0 / total) : 0;

                var name = drive.VolumeLabel.Length > 0 ? drive.VolumeLabel : drive.DriveType.ToString();
                sb.AppendLine($"{name,-30} {FormatFileSize(total),8} {FormatFileSize(used),8} {FormatFileSize(free),8} {usePct,4}%  {drive.RootDirectory}");
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
