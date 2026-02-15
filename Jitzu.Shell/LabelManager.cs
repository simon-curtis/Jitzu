namespace Jitzu.Shell;

public class LabelManager
{
    private readonly Dictionary<string, string> _labels = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> Labels => _labels;

    public void Set(string name, string path)
    {
        _labels[name] = path;
    }

    public bool Remove(string name)
    {
        return _labels.Remove(name);
    }

    /// <summary>
    /// Expands label prefixes in a path. For example, if "git" maps to "D:/git",
    /// then "git:" becomes "D:/git" and "git:jitzu/src" becomes "D:/git/jitzu/src".
    /// </summary>
    public string ExpandLabel(string path)
    {
        var colonIndex = path.IndexOf(':');
        if (colonIndex <= 0)
            return path;

        // Don't expand Windows drive letters (single char like C:)
        if (colonIndex == 1)
            return path;

        var labelName = path[..colonIndex];
        if (!_labels.TryGetValue(labelName, out var labelPath))
            return path;

        var rest = path[(colonIndex + 1)..];
        if (rest.Length == 0)
            return labelPath;

        // Strip leading slash/backslash from rest to avoid double separators
        if (rest[0] is '/' or '\\')
            rest = rest[1..];

        return Path.Join(labelPath, rest);
    }
}
