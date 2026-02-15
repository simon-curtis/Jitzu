using System.Reflection;
using Jitzu.Core.Runtime.Memory;

namespace Jitzu.Core.Runtime;

public record RuntimeProgram
{
    public required Dictionary<string, Type> Types { get; set; }

    // Type resolution caches for namespace support
    public required Dictionary<string, Type> SimpleTypeCache { get; set; }
    public required Dictionary<string, HashSet<string>> TypeNameConflicts { get; set; }
    public required Dictionary<string, string> FileNamespaces { get; set; }

    public required Dictionary<string, Type> Globals { get; set; }
    public required MethodTable MethodTable { get; init; }
    public required Dictionary<string, IShellFunction> GlobalFunctions { get; init; }
    public required Dictionary<string, int> GlobalSlotMap { get; init; }
    public required SlotMapBuilder SlotBuilder { get; set; }
    public HashSet<Assembly> LoadedAssemblies { get; init; } = [];
}