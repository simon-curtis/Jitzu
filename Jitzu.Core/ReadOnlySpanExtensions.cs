using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Jitzu.Core;

public static class ReadOnlySpanExtensions
{
    public static bool All<T>(
        this ReadOnlySpan<T> source,
        Func<T, bool> map) =>
        new ReadOnlySpanSelectEnumerable<T, bool>(source, map).All();

    public static bool All<T, TState>(
        this ReadOnlySpan<T> source,
        TState state,
        Func<TState, T, bool> map) =>
        new ReadOnlySpanSelectStatefulEnumerable<T, TState, bool>(source, state, map).All();

    public static TState Aggregate<T, TState>(
        this ReadOnlySpan<T> source,
        TState state,
        Func<TState, T, TState> aggregate) =>
        new ReadOnlySpanSelectStatefulEnumerable<T, TState, TState>(source, state, aggregate).Aggregate();

    public static bool OfType<T, TType>(this ReadOnlySpan<T> source) =>
        new ReadOnlySpanSelectEnumerable<T, bool>(source, static item => item is TType).All();

    public static string Join(this IEnumerable<string> source, string separator) =>
        string.Join(separator, source);

    public static ReadOnlySpanSelectStatefulEnumerable<T, TState, TOut> Select<T, TState, TOut>(
        this ReadOnlySpan<T> source,
        TState state,
        Func<TState, T, TOut> map) =>
        new(source, state, map);

    public static string Join<T>(
        this ReadOnlySpanSelectEnumerable<T, string> statefulEnumerable,
        char seperator)
    {
        var sb = new StringBuilder();

        var index = 0;
        while (statefulEnumerable.MoveNext())
        {
            if (index++ > 0)
                sb.Append(seperator);

            sb.Append(statefulEnumerable.Current);
        }

        return sb.ToString();
    }
    
    public static string Join<T>(
        this ReadOnlySpanSelectEnumerable<T, string> statefulEnumerable, 
        string seperator)
    {
        var sb = new StringBuilder();

        var index = 0;
        while (statefulEnumerable.MoveNext())
        {
            if (index++ > 0)
                sb.Append(seperator);

            sb.Append(statefulEnumerable.Current);
        }

        return sb.ToString();
    }

    public static List<T> ToList<T>(this ReadOnlySpan<T> span)
    {
        var list = new List<T>(span.Length);
        list.AddRange(span);
        return list;
    }

    public static ReadOnlySpan<char> RemoveChars(this ReadOnlySpan<char> source, char charToRemove)
    {
        Span<char> output = stackalloc char[source.Length];
        var index = 0;
        foreach (var c in source)
        {
            if (c == charToRemove)
                continue;
            output[index++] = c;
        }

        return output[..index].ToArray();
    }

    public static string ReplaceControlCharacters(this ReadOnlySpan<char> input)
    {
        Span<char> output = stackalloc char[input.Length];
        var cursor = 0;

        foreach (var t in input)
        {
            if (char.IsControl(t)) continue;
            output[cursor++] = t;
        }

        return new string(output[..cursor]);
    }

     public static bool FastIntParse(this ReadOnlySpan<char> source, [NotNullWhen(true)] out int? output)
     {
         output = null;

        if (source.IsEmpty)
            return false;

        int index = 0;
        bool isNegative = false;

        // Check for negative sign
        if (source[0] == '-')
        {
            isNegative = true;
            index++;

            // Just a minus sign is invalid
            if (index >= source.Length)
                return false;
        }

        long result = 0;
        bool hasDigits = false;
        const long maxValue = int.MaxValue;
        const long minValueAbs = (long)int.MaxValue + 1; // |int.MinValue|

        while (index < source.Length)
        {
            char c = source[index];

            switch (c)
            {
                // Skip underscores
                case '_':
                    index++;
                    continue;

                // Validate digit
                case < '0' or > '9':
                    return false;
            }

            hasDigits = true;
            int digit = c - '0';

            // Check for overflow before multiplication
            long limit = isNegative ? minValueAbs : maxValue;
            if (result > (limit - digit) / 10)
                return false;

            result = result * 10 + digit;
            index++;
        }

        // Must have at least one digit
        if (!hasDigits)
            return false;

        output = isNegative ? -(int)result : (int)result;
        return true;
    }

    public static double FastParseDouble(this ReadOnlySpan<char> s)
    {
        int index = 0;
        bool isNegative = false;

        // Handle sign
        if (s[0] == '-')
        {
            isNegative = true;
            index++;
        }
        else if (s[0] == '+')
        {
            index++;
        }

        double result = 0;

        // Parse integer part
        while (index < s.Length)
        {
            char c = s[index];

            switch (c)
            {
                case '_':
                    index++;
                    continue;
                case < '0' or > '9':
                    goto ParseDecimal;
            }

            result = result * 10 + (c - '0');
            index++;
        }

        ParseDecimal:
        // Parse fractional part
        if (index < s.Length && s[index] == '.')
        {
            index++;
            double fractionalPart = 0;
            double divisor = 1;

            while (index < s.Length)
            {
                char c = s[index];

                switch (c)
                {
                    case '_':
                        index++;
                        continue;
                    case < '0' or > '9':
                        goto ParseExponent;
                }

                fractionalPart = fractionalPart * 10 + (c - '0');
                divisor *= 10;
                index++;
            }

            result += fractionalPart / divisor;
        }

        ParseExponent:
        // Parse exponent
        if (index >= s.Length || (s[index] != 'e' && s[index] != 'E')) 
            return isNegative ? -result : result;

        index++;

        bool expNegative = false;
        if (index < s.Length)
        {
            switch (s[index])
            {
                case '-':
                    expNegative = true;
                    index++;
                    break;
                case '+':
                    index++;
                    break;
            }
        }

        int exponent = 0;

        while (index < s.Length)
        {
            char c = s[index];

            switch (c)
            {
                case '_':
                    index++;
                    continue;
                case < '0' or > '9':
                    goto ApplyExponent;
            }

            // Prevent exponent overflow
            if (exponent <= 3000)
            {
                exponent = exponent * 10 + (c - '0');
            }

            index++;
        }

        ApplyExponent:
        if (expNegative) exponent = -exponent;

        switch (exponent)
        {
            // Handle extreme exponents
            case > 308:
                return isNegative ? double.NegativeInfinity : double.PositiveInfinity;
            case < -324:
                return isNegative ? -0.0 : 0.0;
            default:
                result *= Math.Pow(10, exponent);
                break;
        }

        return isNegative ? -result : result;
    }
}