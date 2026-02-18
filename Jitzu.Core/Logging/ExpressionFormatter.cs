using Jitzu.Core.Formatting;
using Jitzu.Core.Language;

namespace Jitzu.Core.Logging;

public static class ExpressionFormatter
{
    public static string FormatWithLocation(Expression identifierLiteral)
    {
        return $"{Format(identifierLiteral.Location)}: {Format(identifierLiteral)}";
    }

    public static string Format(SourceSpan sourceSpan)
    {
        var (path, length, start, end) = sourceSpan;
        return $"\"{path}:{start.Line}:{start.Column}:{end.Line}:{end.Column}\"";
    }

    public static string Format(Expression expression)
    {
        var writer = new TreeWriter();
        WriteExpression(expression, writer);
        return writer.ToString().TrimEnd();
    }

    public static void WriteExpression(Expression expression, TreeWriter writer)
    {
        var typeName = expression.GetType().Name;

        switch (expression)
        {
            case ScriptExpression scriptExpression:
                {
                    writer.StartObject(nameof(ScriptExpression));
                    foreach (var child in scriptExpression.Body)
                        WriteExpression(child, writer);
                    writer.EndObject();
                    break;
                }

            case BlockBodyExpression blockBodyExpression:
                {
                    foreach (var child in blockBodyExpression.Expressions)
                        WriteExpression(child, writer);
                    break;
                }

            case UseExpression useExpression:
                {
                    writer.StartObject(nameof(UseExpression));
                    WriteExpression(useExpression.Identifier, writer);
                    writer.EndObject();
                    break;
                }

            case BinaryExpression binaryExpression:
                {
                    writer.StartObject(nameof(BinaryExpression));

                    writer.StartObject(nameof(BinaryExpression.Left), binaryExpression.Left.GetType().Name);
                    WriteExpression(binaryExpression.Left, writer);
                    writer.EndObject();

                    writer.WriteKeyValue("Operator", binaryExpression.Operator.Value);

                    writer.StartObject(nameof(BinaryExpression.Right), binaryExpression.Right.GetType().Name);
                    WriteExpression(binaryExpression.Right, writer);
                    writer.EndObject();

                    writer.EndObject();
                    break;
                }

            case AssignmentExpression assignmentExpression:
                {
                    writer.StartObject(nameof(AssignmentExpression));
                    WriteExpression(assignmentExpression.Left, writer);
                    writer.WriteKeyValue("Operator", assignmentExpression.Operator.Value);
                    WriteExpression(assignmentExpression.Right, writer);
                    writer.EndObject();
                    break;
                }

            case IfExpression ifExpression:
                {
                    writer.StartObject(nameof(IfExpression));
                    WriteExpression(ifExpression.Condition, writer);
                    WriteExpression(ifExpression.Then, writer);
                    if (ifExpression.Else is not null)
                        WriteExpression(ifExpression.Else, writer);
                    writer.EndObject();
                    break;
                }

            case TryExpression tryExpression:
                {
                    writer.StartObject(nameof(TryExpression));
                    WriteExpression(tryExpression.Body, writer);
                    writer.EndObject();
                    break;
                }

            case InlineTryExpression tryChainExpression:
            {
                writer.StartObject(nameof(InlineTryExpression));
                WriteExpression(tryChainExpression.Body, writer);
                writer.EndObject();
                break;
            }

            case LetExpression letExpression:
                {
                    writer.StartObject(nameof(LetExpression));

                    writer.WriteKeyValue("Identifier", letExpression.Identifier.Name,
                        letExpression.Identifier.GetType().Name);

                    if (letExpression.TypeIdentifier is not null)
                    {
                        writer.StartObject(nameof(LetExpression.TypeIdentifier));
                        WriteExpression(letExpression.TypeIdentifier, writer);
                        writer.EndObject();
                    }

                    writer.StartObject(nameof(LetExpression.Value), letExpression.Value.GetType().Name);
                    WriteExpression(letExpression.Value, writer);
                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            case FunctionCallExpression functionCallExpression:
                {
                    writer.StartObject(nameof(FunctionCallExpression));

                    WriteExpression(functionCallExpression.Identifier, writer);

                    if (functionCallExpression.Arguments.Length > 0)
                    {
                        writer.StartObject(nameof(FunctionCallExpression.Arguments));
                        foreach (var argument in functionCallExpression.Arguments)
                            WriteExpression(argument, writer);
                        writer.EndObject();
                    }

                    writer.EndObject();
                    break;
                }

            case IdentifierLiteral identifierLiteral:
                {
                    writer.WriteKeyValue(nameof(IdentifierLiteral), identifierLiteral.Token.Value);
                    break;
                }

            case SimpleMemberAccessExpression chainedIdentifierLiteral:
                {
                    writer.StartObject(nameof(SimpleMemberAccessExpression));
                    WriteExpression(chainedIdentifierLiteral.Object, writer);
                    WriteExpression(chainedIdentifierLiteral.Property, writer);
                    writer.EndObject();
                    break;
                }

            case ModuleExpression moduleExpression:
                {
                    writer.StartObject(nameof(ModuleExpression));
                    WriteExpression(moduleExpression.Identifier, writer);
                    writer.EndObject();
                    break;
                }

            case FunctionDefinitionExpression functionExpression:
                {
                    writer.StartObject(nameof(FunctionDefinitionExpression));
                    WriteExpression(functionExpression.Identifier, writer);
                    WriteExpression(functionExpression.Parameters, writer);
                    if (functionExpression.ReturnType is not null)
                        WriteExpression(functionExpression.ReturnType, writer);
                    foreach (var child in functionExpression.Body)
                        WriteExpression(child, writer);
                    writer.EndObject();
                    break;
                }

            case FunctionParametersExpression functionArgumentsExpression:
                {
                    writer.StartObject(nameof(FunctionParametersExpression));
                    if (functionArgumentsExpression.Self is not null)
                        WriteExpression(functionArgumentsExpression.Self, writer);
                    foreach (var argument in functionArgumentsExpression.Parameters)
                        WriteExpression(argument, writer);
                    writer.EndObject();
                    break;
                }

            case FunctionParameterExpression functionParameterExpression:
                {
                    writer.StartObject(nameof(FunctionParameterExpression));
                    writer.WriteKeyValue("Identifier", functionParameterExpression.Identifier.Name);
                    writer.WriteKeyValue("Type", functionParameterExpression.Type.ToString());
                    writer.EndObject();
                    break;
                }

            case IntLiteral literal:
                {
                    writer.WriteKeyValue(nameof(IntLiteral), literal.Token.Value);
                    break;
                }

            case DoubleLiteral literal:
                {
                    writer.WriteKeyValue(nameof(DoubleLiteral), literal.Token.Value);
                    break;
                }

            case StringLiteral stringLiteral:
                {
                    writer.WriteKeyValue(nameof(StringLiteral), stringLiteral.String);
                    break;
                }

            case InterpolatedStringExpression templateStringLiteral:
                {
                    writer.StartObject(nameof(InterpolatedStringExpression));

                    // writer.WriteKeyValue(nameof(interpolatedStringExpression.Template), templateStringLiteral.Template.Token.Value, templateStringLiteral.GetType().Name);

                    writer.StartObject(nameof(InterpolatedStringExpression.Parts));
                    foreach (var value in templateStringLiteral.Parts)
                        WriteExpression(value.Expression, writer);
                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            case BooleanLiteral booleanLiteral:
                {
                    writer.WriteKeyValue(nameof(BooleanLiteral), booleanLiteral.Token.Value);
                    break;
                }

            case KeywordLiteral keywordLiteral:
                {
                    writer.WriteKeyValue(nameof(KeywordLiteral), keywordLiteral.Token.Value);
                    break;
                }

            case TypeDefinitionExpression typeDefinitionExpression:
                {
                    writer.StartObject(nameof(TypeDefinitionExpression));
                    WriteExpression(typeDefinitionExpression.Identifier, writer);
                    foreach (var field in typeDefinitionExpression.Fields)
                        WriteExpression(field, writer);
                    writer.EndObject();
                    break;
                }

            case FieldDefinitionExpression fieldDefinitionExpression:
                {
                    writer.StartObject(nameof(FieldDefinitionExpression));
                    WriteExpression(fieldDefinitionExpression.Identifier, writer);
                    WriteExpression(fieldDefinitionExpression.Type, writer);
                    if (fieldDefinitionExpression.DefaultValue is { })
                        WriteExpression(fieldDefinitionExpression.DefaultValue, writer);
                    writer.EndObject();
                    break;
                }

            case ObjectInstantiationExpression objectInstantiationExpression:
                {
                    writer.StartObject(nameof(ObjectInstantiationExpression));
                    WriteExpression(objectInstantiationExpression.Identifier, writer);
                    foreach (var field in objectInstantiationExpression.Body.Fields)
                        WriteExpression(field, writer);
                    writer.EndObject();
                    break;
                }

            case ObjectFieldInstantiationExpression objectFieldInstantiationExpression:
                {
                    writer.StartObject(nameof(ObjectFieldInstantiationExpression));
                    WriteExpression(objectFieldInstantiationExpression.Identifier, writer);
                    if (objectFieldInstantiationExpression.Value is not null)
                        WriteExpression(objectFieldInstantiationExpression.Value, writer);
                    writer.EndObject();
                    break;
                }

            case TraitDefinitionExpression traitExpression:
                {
                    writer.StartObject(nameof(TraitDefinitionExpression));
                    writer.WriteKeyValue("name", traitExpression.Identifier.Name);
                    foreach (var function in traitExpression.FunctionSignatures)
                        WriteExpression(function, writer);
                    writer.EndObject();
                    break;
                }

            case TraitFunctionSignature traitFunctionSignature:
                {
                    writer.StartObject(nameof(TraitFunctionSignature));
                    writer.WriteKeyValue("name", traitFunctionSignature.Identifier.ToString());
                    WriteExpression(traitFunctionSignature.Parameters, writer);
                    if (traitFunctionSignature.ReturnType is not null)
                        WriteExpression(traitFunctionSignature.ReturnType, writer);
                    writer.EndObject();
                    break;
                }

            case ImplExpression implExpression:
                {
                    writer.StartObject(nameof(ImplExpression));
                    writer.WriteKeyValue("Trait Name", implExpression.TraitIdentifier.Name);
                    writer.StartObject(nameof(ImplExpression.AssociatedTypes));
                    foreach (var at in implExpression.AssociatedTypes)
                        WriteExpression(at, writer);
                    writer.EndObject();

                    writer.WriteKeyValue("Type Identifier", implExpression.TypeIdentifier.ToString());
                    foreach (var function in implExpression.Functions)
                        WriteExpression(function, writer);
                    writer.EndObject();
                    break;
                }

            case AssociatedTypeImplementationExpression associatedType:
                {
                    writer.StartObject(nameof(AssociatedTypeImplementationExpression));

                    writer.StartObject(nameof(AssociatedTypeImplementationExpression.TypeAlias));
                    WriteExpression(associatedType.TypeAlias, writer);
                    writer.EndObject();

                    writer.StartObject(nameof(AssociatedTypeImplementationExpression.TypeExpression));
                    WriteExpression(associatedType.TypeExpression, writer);
                    writer.EndObject();

                    writer.EndObject();
                    break;
                }

            case ForExpression forExpression:
                {
                    writer.StartObject(nameof(ForExpression));
                    writer.WriteKeyValue("Identifier", forExpression.Identifier.ToString(),
                        forExpression.Identifier.GetType().Name);
                    writer.StartObject(nameof(ForExpression.Range), forExpression.Range.GetType().Name);
                    WriteExpression(forExpression.Range, writer);
                    writer.EndObject();
                    writer.StartObject(nameof(ForExpression.Body), forExpression.Body.GetType().Name);
                    WriteExpression(forExpression.Body, writer);
                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            case RangeExpression rangeExpression:
                {
                    writer.StartObject(nameof(RangeExpression));

                    if (rangeExpression.Left is not null)
                    {
                        writer.StartObject(nameof(RangeExpression.Left));
                        WriteExpression(rangeExpression.Left, writer);
                        writer.EndObject();
                    }

                    writer.WriteKeyValue("Operator", rangeExpression.Operator.Value);

                    if (rangeExpression.Right is not null)
                    {
                        writer.StartObject(nameof(RangeExpression.Right));
                        WriteExpression(rangeExpression.Right, writer);
                        writer.EndObject();
                    }

                    writer.EndObject();
                    break;
                }

            case OperatorLiteral operatorLiteral:
                {
                    writer.WriteKeyValue(nameof(OperatorLiteral), operatorLiteral.Token.Value);
                    break;
                }

            case UnionDefinitionExpression enumDefinitionExpression:
                {
                    writer.StartObject(nameof(UnionDefinitionExpression));
                    writer.WriteKeyValue(nameof(UnionDefinitionExpression.Identifier),
                        enumDefinitionExpression.Identifier.Name);
                    writer.StartObject(nameof(UnionDefinitionExpression.Variants));
                    foreach (var variant in enumDefinitionExpression.Variants)
                        WriteExpression(variant, writer);
                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            case EnumVariantExpression enumVariantExpression:
                {
                    writer.StartObject(nameof(EnumVariantExpression));
                    writer.WriteKeyValue(nameof(EnumVariantExpression.Identifier), enumVariantExpression.Identifier.Name);
                    if (enumVariantExpression.Fields.Length > 0)
                    {
                        writer.StartObject(nameof(EnumVariantExpression.Fields));
                        foreach (var field in enumVariantExpression.Fields)
                            WriteExpression(field, writer);
                        writer.EndObject();
                    }

                    writer.EndObject();
                    break;
                }

            case MatchExpression matchExpression:
                {
                    writer.StartObject(nameof(MatchExpression));
                    WriteExpression(matchExpression.Expression, writer);
                    writer.StartObject(nameof(MatchExpression.Cases));
                    foreach (var caseExpression in matchExpression.Cases)
                        WriteExpression(caseExpression, writer);
                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            case MatchArm matchCase:
                {
                    writer.StartObject(nameof(MatchArm));
                    WriteExpression(matchCase.Pattern, writer);
                    WriteExpression(matchCase.Body, writer);
                    writer.EndObject();
                    break;
                }

            case WhileExpression whileExpression:
                {
                    writer.StartObject(nameof(WhileExpression));
                    WriteExpression(whileExpression.Condition, writer);
                    WriteExpression(whileExpression.Body, writer);
                    writer.EndObject();
                    break;
                }

            case CharLiteral charLiteral:
                {
                    writer.WriteKeyValue(nameof(CharLiteral), charLiteral.Token.Value);
                    break;
                }

            case CommentExpression:
                break;

            case OpenExpression openExpression:
                {
                    writer.WriteKeyValue(nameof(OpenExpression), openExpression.Path.String);
                    break;
                }

            case GenericNameLiteral genericNameLiteral:
                {
                    writer.StartObject(nameof(GenericNameLiteral));
                    writer.WriteKeyValue(nameof(GenericNameLiteral.Identifier), genericNameLiteral.Identifier.Value);
                    writer.StartObject(nameof(GenericNameLiteral.TypeArgumentList));
                    foreach (var arg in genericNameLiteral.TypeArgumentList)
                    {
                        WriteExpression(arg, writer);
                    }

                    writer.EndObject();
                    writer.EndObject();
                    break;
                }

            default:
                writer.WriteNotImplemented(typeName);
                break;
        }
    }
}