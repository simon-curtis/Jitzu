namespace Jitzu.Shell;

public class AliasManager
{
    private readonly string _aliasFile;
    private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly bool _persist;

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public AliasManager(bool persist = true)
    {
        _persist = persist;
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jitzu");
        if (_persist)
            Directory.CreateDirectory(appData);
        _aliasFile = Path.Combine(appData, "aliases.txt");
    }

    public async Task InitialiseAsync()
    {
        if (!_persist || !File.Exists(_aliasFile))
            return;

        var lines = await File.ReadAllLinesAsync(_aliasFile);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
                continue;

            var name = line[..eqIndex].Trim();
            var value = line[(eqIndex + 1)..].Trim();
            _aliases[name] = value;
        }
    }

    public void Set(string name, string value)
    {
        _aliases[name] = value;
    }

    public bool Remove(string name)
    {
        return _aliases.Remove(name);
    }

    public bool TryExpand(string firstWord, out string expanded)
    {
        return _aliases.TryGetValue(firstWord, out expanded!);
    }

    public async Task SaveAsync()
    {
        if (!_persist)
            return;

        var lines = new List<string>(_aliases.Count);
        foreach (var (name, value) in _aliases.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            lines.Add($"{name}={value}");

        await File.WriteAllLinesAsync(_aliasFile, lines);
    }
}
