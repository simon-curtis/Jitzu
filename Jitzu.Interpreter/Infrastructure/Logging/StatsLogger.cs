using System.Diagnostics;

namespace Jitzu.Interpreter.Infrastructure.Logging;

public static class StatsLogger
{
    [Conditional("DEBUG")]
    public static void LogTime(string stage, TimeSpan time) => Console.WriteLine($"\e[90m{stage}\e[0m: {time}");
}