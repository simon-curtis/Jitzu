using Jitzu.Core.Language;

namespace Jitzu.Core;

public class JitzuException(
    SourceSpan location,
    string message
) : Exception(message)
{
    public SourceSpan Location { get; set; } = location;
}

public class FeatureNotImplementedException(Expression expression) 
    : JitzuException(expression.Location, $"Haven't implemented {expression.ToString()} yet");