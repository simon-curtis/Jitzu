using Jitzu.Core;

namespace Jitzu.Shell.Core;

/// <summary>
/// Handles multi-line input accumulation and detects when input is complete.
/// </summary>
public class InputProcessor
{
    private readonly List<string> _accumulatedLines = new();
    private int _openBraces = 0;
    private int _openBrackets = 0;
    private int _openParens = 0;

    public bool IsEmpty => _accumulatedLines.Count == 0;

    public string GetAccumulated() => string.Join("\n", _accumulatedLines);

    public void Clear()
    {
        _accumulatedLines.Clear();
        _openBraces = 0;
        _openBrackets = 0;
        _openParens = 0;
    }

    /// <summary>
    /// Add a line and determine if input is complete.
    /// Returns: (isComplete: bool, reason: string?)
    /// </summary>
    public (bool IsComplete, string? ContinueReason) AddLine(string line)
    {
        _accumulatedLines.Add(line);

        // Track nesting depth
        foreach (char c in line)
        {
            switch (c)
            {
                case '{': _openBraces++; break;
                case '}': _openBraces--; break;
                case '[': _openBrackets++; break;
                case ']': _openBrackets--; break;
                case '(': _openParens++; break;
                case ')': _openParens--; break;
            }
        }

        // Check if we're in the middle of a construct
        if (_openBraces > 0)
            return (false, "unclosed braces");
        if (_openBrackets > 0)
            return (false, "unclosed brackets");
        if (_openParens > 0)
            return (false, "unclosed parentheses");

        // Check for incomplete statements by attempting to parse
        var accumulated = GetAccumulated();

        // Try parsing to detect incomplete constructs
        try
        {
            // Attempt a parse - if it throws, input might be incomplete
            var lexer = new Lexer("<repl>", accumulated);
            var tokens = lexer.Lex();
            var parser = new Parser(tokens);
            parser.Parse(); // Will throw if incomplete

            return (true, null);
        }
        catch (UnexpectedEndOfFileException)
        {
            return (false, "incomplete statement");
        }
        catch (Exception)
        {
            // Any other parsing error means the input is malformed
            // but syntactically complete, so we should try to execute it
            return (true, null);
        }
    }
}
