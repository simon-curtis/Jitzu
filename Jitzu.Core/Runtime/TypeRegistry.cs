namespace Jitzu.Core.Runtime;

/// <summary>
/// Manages type registration and resolution with full namespace support.
/// Supports both simple names (for backward compatibility) and qualified names.
/// </summary>
internal class TypeRegistry
{
    private readonly Dictionary<string, Type> _types;
    private readonly Dictionary<string, Type> _simpleTypeCache;
    private readonly Dictionary<string, HashSet<string>> _typeNameConflicts;

    public IReadOnlyDictionary<string, Type> Types => _types.AsReadOnly();
    public IReadOnlyDictionary<string, Type> SimpleTypeCache => _simpleTypeCache.AsReadOnly();
    public IReadOnlyDictionary<string, HashSet<string>> TypeNameConflicts => _typeNameConflicts.AsReadOnly();

    public TypeRegistry()
    {
        _types = new Dictionary<string, Type>();
        _simpleTypeCache = new Dictionary<string, Type>();
        _typeNameConflicts = new Dictionary<string, HashSet<string>>();
    }

    /// <summary>
    /// Registers a type with its full qualified name.
    /// </summary>
    public void RegisterType(string fullQualifiedName, Type type)
    {
        _types[fullQualifiedName] = type;
    }

    /// <summary>
    /// Builds the simple type cache and conflict tracking after all types are registered.
    /// This should be called once after all types have been registered.
    /// </summary>
    public void BuildCaches()
    {
        _simpleTypeCache.Clear();
        _typeNameConflicts.Clear();

        // Build a map of simple names to full qualified names
        var simpleNameToFullNames = new Dictionary<string, HashSet<string>>();

        foreach (var (fullName, type) in _types)
        {
            var simpleName = ExtractSimpleName(fullName);

            if (!simpleNameToFullNames.TryGetValue(simpleName, out var fullNames))
            {
                fullNames = new HashSet<string>();
                simpleNameToFullNames[simpleName] = fullNames;
            }

            fullNames.Add(fullName);
        }

        // Populate cache and conflicts
        foreach (var (simpleName, fullNames) in simpleNameToFullNames)
        {
            if (fullNames.Count == 1)
            {
                // Unambiguous - add to cache
                var fullName = fullNames.Single();
                _simpleTypeCache[simpleName] = _types[fullName];
            }
            else
            {
                // Ambiguous - track for error reporting
                _typeNameConflicts[simpleName] = fullNames;
            }
        }
    }

    /// <summary>
    /// Resolves a simple type name. Returns the type if unambiguous, throws if ambiguous.
    /// </summary>
    public Type? ResolveSimpleName(string simpleName)
    {
        // Fast path: check cache
        if (_simpleTypeCache.TryGetValue(simpleName, out var type))
        {
            return type;
        }

        // Check for conflicts
        if (_typeNameConflicts.TryGetValue(simpleName, out var fullNames))
        {
            var suggestions = string.Join(", ", fullNames.OrderBy(x => x));
            throw new InvalidOperationException(
                $"Type '{simpleName}' is ambiguous. Did you mean: {suggestions}?");
        }

        return null;
    }

    /// <summary>
    /// Resolves a qualified type name.
    /// </summary>
    public Type? ResolveQualifiedName(string qualifiedName)
    {
        return _types.TryGetValue(qualifiedName, out var type) ? type : null;
    }

    /// <summary>
    /// Extracts the simple name from a fully qualified name.
    /// E.g., "System.Text.Json.JsonSerializer" -> "JsonSerializer"
    /// </summary>
    private static string ExtractSimpleName(string fullQualifiedName)
    {
        var lastDot = fullQualifiedName.LastIndexOf('.');
        return lastDot < 0 ? fullQualifiedName : fullQualifiedName[(lastDot + 1)..];
    }

    /// <summary>
    /// Flattens a member access expression chain into a qualified name string.
    /// </summary>
    public static string FlattenMemberAccess(IEnumerable<string> parts)
    {
        return string.Join(".", parts);
    }
}
