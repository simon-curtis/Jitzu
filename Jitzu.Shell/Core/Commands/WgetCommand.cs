using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Downloads files from URLs.
/// </summary>
public class WgetCommand : CommandBase
{
    public WgetCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        string? url = null;
        string? outputPath = null;
        var quiet = false;

        var span = args.Span;
        for (var i = 0; i < span.Length; i++)
        {
            var arg = span[i];
            if (arg is "-O" or "--output-document" or "-o")
            {
                if (i + 1 >= span.Length)
                    return new ShellResult(ResultType.Error, "", new Exception("wget: option requires an argument -- 'O'"));
                outputPath = ExpandPath(span[++i]);
            }
            else if (arg is "-q" or "--quiet")
            {
                quiet = true;
            }
            else if (arg.StartsWith("-"))
            {
                return new ShellResult(ResultType.Error, "", new Exception($"wget: unrecognized option '{arg}'"));
            }
            else
            {
                url = arg;
            }
        }

        if (url is null)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: wget [-q] [-O <file>] <url>"));

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new ShellResult(ResultType.Error, "", new Exception($"wget: invalid URL '{url}'"));

        outputPath ??= ExpandPath(Path.GetFileName(uri.AbsolutePath) is { Length: > 0 } name ? name : "index.html");

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Jitzu-Shell/1.0");

            var output = new StringBuilder();
            if (!quiet)
                output.AppendLine($"--  {uri}");

            using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            if (!quiet)
                output.AppendLine($"HTTP request sent, awaiting response... {(int)response.StatusCode} {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
                return new ShellResult(ResultType.Error, output.ToString(),
                    new Exception($"wget: server returned {(int)response.StatusCode} {response.ReasonPhrase}"));

            var contentLength = response.Content.Headers.ContentLength;
            if (!quiet && contentLength.HasValue)
                output.AppendLine($"Length: {contentLength.Value} ({FormatBytes(contentLength.Value)})");

            if (!quiet)
                output.AppendLine($"Saving to: '{outputPath}'");

            await using var fileStream = File.Create(outputPath);
            await using var httpStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            while ((bytesRead = await httpStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;
            }

            if (!quiet)
            {
                output.AppendLine();
                output.Append($"'{outputPath}' saved [{totalRead}");
                if (contentLength.HasValue)
                    output.Append($"/{contentLength.Value}");
                output.AppendLine("]");
            }

            return new ShellResult(ResultType.OsCommand, output.ToString(), null);
        }
        catch (HttpRequestException ex)
        {
            return new ShellResult(ResultType.Error, "", new Exception($"wget: {ex.Message}"));
        }
        catch (TaskCanceledException)
        {
            return new ShellResult(ResultType.Error, "", new Exception("wget: connection timed out"));
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
