using Jitzu.Core.Language;

namespace Jitzu.Core.Formatting;

public static class TokenFormatter
{
    public static string Format(Token token, bool includeFilePath = true)
    {
        return includeFilePath
            ? $"{token.Span.FilePath}:{token.Span.Start.Line}:{token.Span.Start.Column}:{token.Span.End.Line}:{token.Span.End.Column} {token.Type.ToStringFast()}: {token.Value}"
            : $"{token.Type.ToStringFast()} {token.Value}";
    }
}