using System.Reflection;
using System.Runtime.Loader;

namespace Jitzu.Core.Runtime.Compilation;

public sealed class RuntimeLoadContext(string[] assemblyPaths) : AssemblyLoadContext(isCollectible: false)
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        foreach (var path in assemblyPaths)
        {
            if (Path.GetFileNameWithoutExtension(path)
                .Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return LoadFromAssemblyPath(path);
            }
        }

        return null;
    }
}