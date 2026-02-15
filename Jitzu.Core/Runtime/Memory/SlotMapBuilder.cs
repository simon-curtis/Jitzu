using Jitzu.Core.Language;

namespace Jitzu.Core.Runtime.Memory;

public record struct Local(LocalKind LocalKind, int SlotIndex);

public class SlotMapBuilder(SlotMapBuilder? parentBuilder, LocalKind localKind)
{
    private Dictionary<string, int>[] _scopes = new Dictionary<string, int>[32];
    private int _scopeIndex = -1;
    private int _slotIndex;

    /// <summary>
    /// Push a new lexical scope (block or function body).
    /// Does NOT reset slot index — slots are unique per function.
    /// </summary>
    public Dictionary<string, int> PushScope()
    {
        if (++_scopeIndex >= _scopes.Length)
            Array.Resize(ref _scopes, _scopes.Length * 2);

        return _scopes[_scopeIndex] = new Dictionary<string, int>();
    }

    public void PopScope()
    {
        --_scopeIndex;
    }

    public Local Add()
    {
        var local = new Local(localKind, _slotIndex);
        _slotIndex++;
        return local;
    }

    /// <summary>
    /// Add a new local variable in the current scope.
    /// Always increments slot index — no reuse within a function.
    /// </summary>
    public Local Add(string name)
    {
        var scope = _scopes[_scopeIndex];
        if (scope.TryGetValue(name, out var slot))
            return new Local(localKind, slot);

        var local = new Local(localKind, _slotIndex);
        scope.Add(name, _slotIndex++);
        return local;
    }

    /// <summary>
    /// Try to find a local in the current function or enclosing functions.
    /// </summary>
    public bool TryGetLocal(string name, out Local local)
    {
        // Search current function's scopes
        for (var index = _scopeIndex; index >= 0; index--)
        {
            if (!_scopes[index].TryGetValue(name, out var slot)) 
                continue;

            local = new Local(localKind, slot);
            return true;
        }

        // Search enclosing functions
        if (parentBuilder != null)
        {
            if (parentBuilder.TryGetLocal(name, out var enclosingLocal))
            {
                local = enclosingLocal.LocalKind is LocalKind.Local
                    ? enclosingLocal with
                    {
                        LocalKind = LocalKind.Upvalue,
                    }
                    : enclosingLocal;

                return true;
            }
        }

        local = default;
        return false;
    }

    /// <summary>
    /// Get the total number of locals allocated in this function.
    /// </summary>
    public int LocalCount => _slotIndex;
}