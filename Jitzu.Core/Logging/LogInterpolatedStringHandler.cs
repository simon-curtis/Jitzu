using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Jitzu.Core.Language;

namespace Jitzu.Core.Logging;

[InterpolatedStringHandler]
public readonly struct LogInterpolatedStringHandler
{
    private readonly int _formattedCount;
    private readonly StringBuilder _builder = new();

    public LogInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _formattedCount = formattedCount;
        _builder = new StringBuilder(literalLength);
    }

    public void AppendFormatted(ReadOnlySpan<char> s) => _builder.Append(s);
    public void AppendFormatted(int i) => _builder.Append(i);

    [OverloadResolutionPriority(-1)]
    public void AppendFormatted(object s) => _builder.Append(ValueFormatter.Format(s));

    public void AppendLiteral(ReadOnlySpan<char> s) => _builder.Append(s);
    public void AppendFormatted(Expression expression) => _builder.Append(ExpressionFormatter.Format(expression));
    public void AppendFormatted(TypeInfo typeInfo) => _builder.Append(ValueFormatter.Format(typeInfo));
    internal string GetFormattedText() => _builder.ToString();
}