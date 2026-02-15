using System.Reflection;
using System.Runtime.Loader;

namespace Jitzu.Shell.Core;

public sealed class ReplLoadContext : AssemblyLoadContext
{
    private readonly Dictionary<string, string> _assemblies = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterAssemblyPath(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        _assemblies[name] = path;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_assemblies.TryGetValue(assemblyName.Name!, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}