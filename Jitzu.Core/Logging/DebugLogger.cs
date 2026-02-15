using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jitzu.Core.Formatting;
using Jitzu.Core.Language;

namespace Jitzu.Core.Logging;

public static class DebugLogger
{
    private static bool _isEnabled;

    [Conditional("DEBUG")]
    public static void SetIsEnabled(bool enabled) => _isEnabled = enabled;

    [Conditional("DEBUG")]
    public static void WriteLine(
        string message,
        [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int? calledLinerNumber = null)
    {
        if (!_isEnabled || callerFileName is null) return;
        Console.WriteLine($"\e[90m{callerFileName}:{calledLinerNumber}: {message}\e[0m");
    }

    [Conditional("DEBUG")]
    public static void WriteLine(
        LogInterpolatedStringHandler message,
        [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int? calledLinerNumber = null)
    {
        if (!_isEnabled || callerFileName is null) return;
        Console.WriteLine($"\e[90m{callerFileName}:{calledLinerNumber}: {message.GetFormattedText()}\e[0m");
    }

    [Conditional("DEBUG")]
    public static void WriteTokens(
        List<Token> tokens,
        [CallerFilePath] string? callerFileName = null,
        [CallerLineNumber] int? calledLinerNumber = null)
    {
        if (!_isEnabled || callerFileName is null) return;
        foreach (var token in tokens)
            Console.WriteLine($"\e[90m{callerFileName}:{calledLinerNumber}: {TokenFormatter.Format(token)}\e[0m");
    }
}