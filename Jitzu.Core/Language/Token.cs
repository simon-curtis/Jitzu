using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Jitzu.Core.Language;

public readonly struct Token
{
    public SourceSpan Span { get; init; }
    public string Value { get; init; }
    public TokenType Type { get; init; }

    public static Token Create(
        SourceSpan span,
        ReadOnlySpan<char> value,
        TokenType type
    ) => new()
    {
        Span = span,
        Value = value.ToString(),
        Type = type
    };

    public static Token Create(
        SourceSpan span,
        string value,
        TokenType type
    ) => new()
    {
        Span = span,
        Value = value,
        Type = type
    };

    internal SourceSpan To(Token end)
    {
        return Span.Extend(end.Span.End);
    }
}

public struct Location(int column, int line)
{
    public static readonly Location Empty = new(0, 0);
    public int Line { get; set; } = line;
    public int Column { get; set; } = column;
}

public static class LocationExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AdvanceBy(this ref Location location, int by)
    {
        location.Column += by;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NewLine(this ref Location location)
    {
        location.Column = 1;
        location.Line++;
    }
}

public readonly struct SourceSpan(ReadOnlySpan<char> filePath, int length, Location start, Location end)
{
    public static readonly SourceSpan Empty = new([], 0, Location.Empty, Location.Empty);
    public string FilePath { get; } = filePath.ToString();
    public int Length { get; } = length;
    public Location Start { get; } = start;
    public Location End { get; } = end;
    public SourceSpan Extend(Location location) => new(FilePath, Length, Start, location);
    public SourceSpan Extend(SourceSpan location) => new(FilePath, Length, Start, location.End);

    public bool IsOnSameLine(SourceSpan other)
    {
        return FilePath == other.FilePath && Start.Line == other.Start.Line;
    }

    public void Deconstruct(out string filePath, out int length, out Location start, out Location end)
    {
        filePath = FilePath;
        length = Length;
        start = Start;
        end = End;
    }

    public SourceSpan Relative(SourceSpan innerRange)
    {
        return new SourceSpan(
            FilePath,
            innerRange.Length,
            new Location(Start.Column + innerRange.Start.Column, Start.Line),
            new Location(Start.Column + innerRange.End.Column, Start.Line));
    }

    public override string ToString() => $"{FilePath}[{Start.Line}:{Start.Column}..{End.Line}:{End.Column}]";
}

public static class CommonTokens
{
    public static Token PlusToken(SourceSpan location)
    {
        return new Token
        {
            Value = "+",
            Span = location,
            Type = TokenType.Operator,
        };
    }

    public static Token MinusToken(SourceSpan location)
    {
        return new Token
        {
            Value = "-",
            Span = location,
            Type = TokenType.Operator,
        };
    }
}