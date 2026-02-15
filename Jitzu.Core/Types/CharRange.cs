using System.Collections;

namespace Jitzu.Core.Types;

public record CharRange(char Start, char End, bool Inclusive) : IEnumerable<object>
{
    public IEnumerator<object> GetEnumerator()
    {
        // Custom comparer: lowercase before uppercase, then normal ordering
        int CustomCompare(char a, char b)
        {
            bool aLower = char.IsLower(a);
            bool bLower = char.IsLower(b);

            return aLower switch
            {
                true when !bLower => -1,
                false when bLower => 1,
                _ => a.CompareTo(b)
            };
        }

        char current = Start;
        while (true)
        {
            // Stop condition
            if (Inclusive)
            {
                if (CustomCompare(current, End) > 0) yield break;
            }
            else
            {
                if (CustomCompare(current, End) >= 0) yield break;
            }

            yield return current;

            // Move to next character in custom order
            current = NextChar(current);
        }
    }

    private static char NextChar(char c)
    {
        // If lowercase, go to next lowercase until 'z', then jump to 'A'
        if (char.IsLower(c))
        {
            if (c < 'z') return (char)(c + 1);
            return 'A'; // after 'z', go to 'A'
        }

        // If uppercase, go to next uppercase until 'Z'
        if (char.IsUpper(c))
        {
            if (c < 'Z') return (char)(c + 1);
        }

        // Otherwise, just increment normally
        return (char)(c + 1);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}