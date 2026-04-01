namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Renders a file with formatting and displays it in the pager.
/// Markdown files get ANSI formatting; other files display as plain text.
/// </summary>
public class ViewCommand(CommandContext context) : CommandBase(context)
{
    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: view <file>"));

        var path = ExpandPath(args.Span[0]);

        if (!File.Exists(path))
            return new ShellResult(ResultType.Error, "", new Exception($"File not found: {args.Span[0]}"));

        var lines = await File.ReadAllLinesAsync(path);

        var ext = Path.GetExtension(path).ToLowerInvariant();
        var output = ext is ".md" or ".markdown"
            ? string.Join('\n', MarkdownRenderer.Render(lines))
            : string.Join('\n', lines);

        Context.BuiltinCommands!.SetPagerInput(output);
        return await Context.BuiltinCommands.ExecuteAsync("more", ReadOnlyMemory<string>.Empty);
    }
}
