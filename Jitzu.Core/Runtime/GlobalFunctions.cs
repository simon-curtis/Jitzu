using System.Runtime.CompilerServices;
using Jitzu.Core.Logging;
using Jitzu.Core.Types;

namespace Jitzu.Core.Runtime;

public static class GlobalFunctions
{
    public static object Or(this object instance, object fallback)
    {
        return instance switch
        {
            ICanFallback f => f.Fallback(fallback),
            _ => instance
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void PrintStatic(params object?[] objects)
    {
        switch (objects.Length)
        {
            case 0:
                Console.WriteLine();
                break;
            case 1:
                Console.WriteLine(ValueFormatter.Format(objects[0]));
                break;
            default:
                Console.WriteLine(ValueFormatter.Format(objects));
                break;
        }
    }

    public static int RandStatic(object?[] objects)
    {
        return objects switch
        {
            [int max] => Random.Shared.Next(max),
            [int max, int min] => Random.Shared.Next(max, min),
            _ => Random.Shared.Next(),
        };
    }

    public static string FirstStatic(string input)
    {
        var lines = SplitLines(input);
        return lines.Length > 0 ? lines[0] : "";
    }

    public static string LastStatic(string input)
    {
        var lines = SplitLines(input);
        return lines.Length > 0 ? lines[^1] : "";
    }

    public static string NthStatic(string input, int index)
    {
        var lines = SplitLines(input);
        return index >= 0 && index < lines.Length ? lines[index] : "";
    }

    public static string GrepStatic(string input, string pattern)
    {
        var lines = SplitLines(input);
        var matched = lines.Where(line => line.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        return string.Join('\n', matched);
    }

    private static string[] SplitLines(string input)
    {
        return input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
    }
}