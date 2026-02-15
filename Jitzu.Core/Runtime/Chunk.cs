using Jitzu.Core.Language;

namespace Jitzu.Core.Runtime;

public sealed class Chunk
{
    public List<byte> Code { get; } = [];
    public List<object> Constants { get; } = [];
    public Dictionary<int, SourceSpan> DebugSpans { get; } = new();

    private readonly Dictionary<object, int> _constantMap = new();

    public static Label NewLabel() => new();

    public int AddOrGetConstant(object value)
    {
        if (_constantMap.TryGetValue(value, out var idx))
            return idx;

        Constants.Add(value);
        var index = Constants.Count - 1;
        _constantMap.Add(value, index);
        return index;
    }

    public int Emit(OpCode op, SourceSpan span, params ReadOnlySpan<int> operands)
    {
        int offset = Code.Count;

        // Emit the opcode
        Code.Add((byte)op);

        // Emit operands (little-endian assumed)
        foreach (var operand in operands)
            Code.AddRange(BitConverter.GetBytes(operand));

        // Record the span for the *start offset* of this instruction
        DebugSpans[offset] = span;
        return offset;
    }

    public int EmitJump(OpCode op, SourceSpan span, Label target)
    {
        int offset = Code.Count;
        Code.Add((byte)op);

        // Write placeholder
        const int placeholder = -1;
        Code.AddRange(BitConverter.GetBytes(placeholder));

        DebugSpans[offset] = span;

        // Record this as a patch site for the label
        target.PatchSites.Add(offset);

        return offset;
    }

    public void MarkLabel(Label label)
    {
        label.Position = Code.Count;

        foreach (int patchSite in label.PatchSites)
        {
            byte[] bytes = BitConverter.GetBytes(label.Position);

            // operand starts immediately after opcode
            for (int i = 0; i < 4; i++)
                Code[patchSite + 1 + i] = bytes[i];
        }

        label.PatchSites.Clear();
    }
}

public class Label
{
    internal int Position = -1;
    internal readonly List<int> PatchSites = [];
}