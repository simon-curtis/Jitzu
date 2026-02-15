namespace Jitzu.Shell;

public class HistoryManager
{
    private static readonly HashSet<string> IgnoredCommands = ["exit", "clear"];

    private readonly string _historyFile;
    private List<string> _history = [];
    private readonly HashSet<string> _historySet = [];

    public int Count => _history.Count;
    public IEnumerable<char> this[int historyIndex] => _history[historyIndex];

    public HistoryManager()
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jitzu");
        Directory.CreateDirectory(appData);
        _historyFile = Path.Combine(appData, "history.txt");
    }

    public async Task InitialiseAsync()
    {
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
        _history = deduplicated;
        _historySet.UnionWith(_history);
    }

    public int SearchBackward(string query, int startIndex)
    {
        if (string.IsNullOrEmpty(query))
            return -1;

        for (var i = startIndex; i >= 0; i--)
        {
            if (_history[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public string GetEntry(int index) => _history[index];

    public List<string> GetPredictions(string prefix, int maxCount)
    {
        if (string.IsNullOrEmpty(prefix)) return [];

        var results = new List<string>(maxCount);
        for (var i = _history.Count - 1; i >= 0 && results.Count < maxCount; i--)
        {
            if (_history[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && !_history[i].Equals(prefix, StringComparison.OrdinalIgnoreCase)
                && !IgnoredCommands.Contains(_history[i]))
                results.Add(_history[i]);
        }
        return results;
    }

    public async Task RemoveAsync(string entry)
    {
        if (!_historySet.Remove(entry))
            return;

        _history.Remove(entry);

        // Rewrite the file without the removed entry
        await File.WriteAllLinesAsync(_historyFile, _history);
    }

    public async Task WriteAsync(string historyItem)
    {
        if (string.IsNullOrWhiteSpace(historyItem))
            return;

        // Update in-memory history
        if (!_historySet.Add(historyItem))
            _history.Remove(historyItem);

        _history.Add(historyItem);

        // Append to file instead of rewriting entire history
        await File.AppendAllTextAsync(_historyFile, historyItem + Environment.NewLine);
    }
}
