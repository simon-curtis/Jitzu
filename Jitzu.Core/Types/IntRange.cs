using System.Collections;

namespace Jitzu.Core.Types;

public record IntRange(int Start, int End, bool Inclusive) : IEnumerable<object>
{
    public IEnumerator<object> GetEnumerator()
    {
        var end = Inclusive ? End + 1 : End;
        for (var i = Start; i < end; i++)
            yield return i;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}