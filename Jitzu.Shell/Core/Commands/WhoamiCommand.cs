namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays the current user name.
/// </summary>
public class WhoamiCommand : CommandBase
{
    public WhoamiCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var user = Environment.UserName;
        var domain = Environment.UserDomainName;
        var output = domain != user ? $"{domain}\\{user}" : user;
        return Task.FromResult(new ShellResult(ResultType.OsCommand, output, null));
    }
}
