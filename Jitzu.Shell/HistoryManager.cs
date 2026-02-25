namespace Jitzu.Shell;

public class HistoryManager
{
    private static readonly HashSet<string> IgnoredCommands = ["exit", "clear"];

    private readonly string _historyFile;
    private readonly bool _persist;
    private readonly LinkedList<string> _history = new();
    private readonly Dictionary<string, LinkedListNode<string>> _historyIndex = new();

    public int Count => _history.Count;
    public IEnumerable<char> this[int historyIndex] => GetEntry(historyIndex);

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
            var node = _history.AddLast(entry);
            _historyIndex[entry] = node;
        }
    }

    public int SearchBackward(string query, int startIndex)
    {
        if (string.IsNullOrEmpty(query))
            return -1;

        var node = GetNodeAt(startIndex);
        var index = startIndex;

        while (node is not null)
        {
            if (node.Value.Contains(query, StringComparison.OrdinalIgnoreCase))
                return index;

            node = node.Previous;
            index--;
        }

        return -1;
    }

    public string GetEntry(int index)
    {
        var node = GetNodeAt(index);
        return node?.Value ?? throw new ArgumentOutOfRangeException(nameof(index));
    }

    public List<string> GetPredictions(string prefix, int maxCount)
    {
        if (string.IsNullOrEmpty(prefix)) return [];

        var results = new List<string>(maxCount);
        var node = _history.Last;

        while (node is not null && results.Count < maxCount)
        {
            if (node.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && !node.Value.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                && !IgnoredCommands.Contains(node.Value))
                results.Add(node.Value);

            node = node.Previous;
        }

        return results;
    }

    public async Task RemoveAsync(string entry)
    {
        if (!_historyIndex.Remove(entry, out var node))
            return;

        _history.Remove(node);

        if (_persist)
            await File.WriteAllLinesAsync(_historyFile, _history);
    }

    public async Task WriteAsync(string historyItem)
    {
        if (string.IsNullOrWhiteSpace(historyItem))
            return;

        // Move existing entry to end, or add new
        if (_historyIndex.TryGetValue(historyItem, out var existing))
            _history.Remove(existing);

        _historyIndex[historyItem] = _history.AddLast(historyItem);

        if (_persist)
            await File.AppendAllTextAsync(_historyFile, historyItem + Environment.NewLine);
    }

    private LinkedListNode<string>? GetNodeAt(int index)
    {
        if (index < 0 || index >= _history.Count)
            return null;

        var node = _history.First;
        for (var i = 0; i < index; i++)
            node = node!.Next;

        return node;
    }
}
