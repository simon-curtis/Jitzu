using System.Collections.Concurrent;
using System.Diagnostics;

namespace Jitzu.Interpreter.Infrastructure.Logging;

public static class Telemetry
{
    public static bool IsEnabled { get; private set; }

    private static readonly ConcurrentDictionary<string, int> ExpressionCounts = new();
    private static readonly ConcurrentDictionary<string, int> MethodCounts = new();

    [Conditional("DEBUG")]
    public static void SetIsEnabled(bool enabled) => IsEnabled = enabled;

    [Conditional("DEBUG")]
    public static void Expression(string name) => ExpressionCounts.AddOrUpdate(name, 1, (_, i) => i + 1);

    public static IEnumerable<KeyValuePair<string, int>> ExpressionCountResults() =>
        ExpressionCounts.OrderByDescending(_ => _.Value);

    public static void Method(string name) => MethodCounts.AddOrUpdate(name, 1, (_, i) => i + 1);
}
