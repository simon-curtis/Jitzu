namespace Jitzu.Shell.Core.Completions;

public class CompletionComparer : IComparer<Completion>
{
    public static readonly CompletionComparer Instance = new();

    public int Compare(Completion? x, Completion? y) => (x, y) switch
    {
        (null, null) => 0,
        (null, _) => 1,
        (_, null) => -1,
        _ => x.Priority.CompareTo(y.Priority) switch
        {
            var priority and not 0 => priority,
            _ => string.Compare(x.Value, y.Value, StringComparison.OrdinalIgnoreCase)
        }
    };
}