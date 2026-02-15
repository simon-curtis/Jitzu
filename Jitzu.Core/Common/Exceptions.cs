using System.Reflection;
using System.Text;
using Jitzu.Core.Language;
using Jitzu.Core.Logging;

namespace Jitzu.Core.Common;

public class VariableNotFoundException(Expression identifier)
    : JitzuException(identifier.Location, $"R001: Variable {identifier.ToString()} not found");

public class TraitNotImplementedException(string traitName, string typeName, SourceSpan location)
    : JitzuException(location, $"R002: {traitName} is not implemented on type {typeName}");

public class PropertyMissingException(SourceSpan callSite, object key, TypeInfo typeInfo)
    : JitzuException(
        callSite,
        $"R003: Property '{ValueFormatter.Format(key)}' was not found on object type '{typeInfo.Name}'");

public class FunctionNotFoundException(
    SourceSpan callSite,
    string functionName,
    string typeName,
    ReadOnlySpan<Type> typeArgs)
    : JitzuException(callSite, FormatMessage(functionName, typeName, typeArgs))
{
    private static string FormatMessage(string functionName, string typeName, ReadOnlySpan<Type> args)
    {
        var sb = new StringBuilder($"R004: Function {functionName}(");

        for (var index = 0; index < args.Length; index++)
        {
            if (index > 0) sb.Append(", ");
            sb.Append(args[index].Name);
        }

        sb.Append($" not found on type '{typeName}'");
        return sb.ToString();
    }
}

public class InvalidIndexerException(SourceSpan callSite, string indexerType)
    : JitzuException(callSite, $"R005: Invalid indexer type {indexerType}");

public class IndexerNotDefinedException(SourceSpan callSite, string indexerType, string typeName)
    : JitzuException(callSite, $"R006: Indexer for {indexerType} not found on type '{typeName}'");

public class UnwrapException(string errValue, SourceSpan callSite)
    : JitzuException(callSite, $"R007: {errValue}");

public class InvalidObjectSelectorException(Expression expression)
    : JitzuException(expression.Location, $"RO08: Invalid object selector {ExpressionFormatter.Format(expression)}");

public class TypeMismatchException(SourceSpan callSite, TypeInfo received, TypeInfo target)
    : JitzuException(callSite, $"Type mismatch: '{received.Name}' cannot be assigned to '{target.Name}'");

public class UnsupportedExpressionException(Expression expression)
    : JitzuException(expression.Location, $"I don't know how to interpret \e[34m{expression.ToString()}\e[0m");

public class UnknownTypeNameException(Expression expression)
    : JitzuException(expression.Location, "Unknown type name");

public class SymbolNotInstantiableException(Type symbol, SourceSpan location)
    : JitzuException(location, $"{symbol.Name} is not instantiable");

public class UnsupportedMemberAccessException(Expression expression)
    : JitzuException(
        expression.Location,
        $"Expression {expression.ToString()} is not supported as a simple member access expression");

public class ValueNotIterableException(Expression expression, object other)
    : JitzuException(expression.Location, $"Value {other.GetType().Name} is not iterable");

public class SymbolNotFoundException(Expression typeExpression)
    : JitzuException(typeExpression.Location, $"Symbol {typeExpression.ToString()} could not be found");