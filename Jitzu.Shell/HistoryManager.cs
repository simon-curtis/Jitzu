namespace Jitzu.Shell;

public class HistoryManager
{
    private static readonly HashSet<string> IgnoredCommands = ["exit", "clear"];

    private readonly string _historyFile;
    private readonly bool _persist;
    private readonly List<string> _history = [];
    private readonly HashSet<string> _historySet = [];

    public int Count => _history.Count;
    public string this[int historyIndex] => _history[historyIndex];

    public HistoryManager(bool persist = true)
    {
        _persist = persist;
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jitzu");
        if (_persist)
            Directory.CreateDirectory(appData);
        _historyFile = Path.Combine(appData, "history.txt");
    }

    public async Task InitialiseAsync()
    {
        if (!_persist)
            return;

        if (!File.Exists(_historyFile))
        {
            await File.WriteAllTextAsync(_historyFile, "");
            return;
        }

        var lines = await File.ReadAllLinesAsync(_historyFile);

        // Deduplicate on load - keep only the last occurrence of each command
        var deduplicated = new List<string>();
        var seen = new HashSet<string>();

        for (var i = lines.Length - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]) && seen.Add(lines[i]))
                deduplicated.Add(lines[i]);
        }

        deduplicated.Reverse();

        foreach (var entry in deduplicated)
        {
            _history.Add(entry);
            _historySet.Add(entry);
        }
    }

    public int SearchBackward(string query, int startIndex)
    {
        if (string.IsNullOrEmpty(query))
            return -1;

        for (var i = Math.Min(startIndex, _history.Count - 1); i >= 0; i--)
        {
            if (_history[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public List<string> GetPredictions(ReadOnlySpan<char> prefix, int maxCount, Func<string, bool>? filter = null)
    {
        if (prefix.IsEmpty) return [];

        var results = new List<string>(maxCount);

        for (var i = _history.Count - 1; i >= 0 && results.Count < maxCount; i--)
        {
            var entry = _history[i];
            if (entry.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && !entry.AsSpan().Equals(prefix, StringComparison.OrdinalIgnoreCase)
                && !IgnoredCommands.Contains(entry)
                && (filter is null || filter(entry)))
                results.Add(entry);
        }

        return results;
    }

    public async Task RemoveAsync(string entry)
    {
        if (!_historySet.Remove(entry))
            return;

        _history.Remove(entry);

        if (_persist)
            await File.WriteAllLinesAsync(_historyFile, _history);
    }

    public async Task WriteAsync(string historyItem)
    {
        if (string.IsNullOrWhiteSpace(historyItem))
            return;

        // Move existing entry to end, or add new
        if (!_historySet.Add(historyItem))
            _history.Remove(historyItem);

        _history.Add(historyItem);

        if (_persist)
            await File.AppendAllTextAsync(_historyFile, historyItem + Environment.NewLine);
    }
}
