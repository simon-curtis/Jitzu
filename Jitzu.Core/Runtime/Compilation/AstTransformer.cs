using Jitzu.Core.Language;
using Jitzu.Core.Runtime.Memory;

namespace Jitzu.Core.Runtime.Compilation;

/// <summary>
/// Transforms AST expressions to use LocalGet and LocalSet expressions for local variable access.
/// This enables efficient local variable storage using slot indices instead of dictionary lookups.
/// </summary>
public class AstTransformer(RuntimeProgram program)
{
    public void TransformScriptExpression(
        ScriptExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        // Don't add a scope here, this should be extending the program scope
        for (int i = 0; i < expression.Body.Length; i++)
            expression.Body[i] = TransformExpression(expression.Body[i], slotMapBuilder);
    }

    public Dictionary<string, int> TransformFunctionBody(
        FunctionDefinitionExpression fun,
        SlotMapBuilder? slotMapBuilder)
    {
        var scopedSlotMap = new SlotMapBuilder(slotMapBuilder, LocalKind.Local);
        var slotMap = scopedSlotMap.PushScope();

        // Reserve slots for parameters (including self if present)
        if (fun.Parameters.Self != null)
            scopedSlotMap.Add("self");

        foreach (var param in fun.Parameters.Parameters)
            scopedSlotMap.Add(param.Identifier.Name);

        for (int i = 0; i < fun.Body.Length; i++)
            fun.Body[i] = TransformExpression(fun.Body[i], scopedSlotMap);

        return slotMap;
    }

    /// <summary>
    /// Transforms an expression to use locals where appropriate.
    /// </summary>
    private Expression TransformExpression(
        Expression expr,
        SlotMapBuilder slotMapBuilder)
    {
        return expr switch
        {
            LetExpression e => TransformLetExpression(e, slotMapBuilder),
            AssignmentExpression e => TransformAssignmentExpression(e, slotMapBuilder),
            // Check for qualified type name before treating as normal member access
            SimpleMemberAccessExpression e when TryTransformQualifiedTypeName(e, slotMapBuilder, out var result) => result,
            SimpleMemberAccessExpression e => TransformSimpleMemberAccess(e, slotMapBuilder),
            BlockBodyExpression e => TransformBlockBody(e, slotMapBuilder),
            FunctionCallExpression e => TransformFunctionCallExpression(e, slotMapBuilder),
            BinaryExpression e => TransformBinaryExpression(e, slotMapBuilder),
            IfExpression e => TransformIfExpression(e, slotMapBuilder),
            IndexerExpression e => TransformIndexerExpression(e, slotMapBuilder),
            TryExpression e => TransformTryExpression(e, slotMapBuilder),
            ObjectInstantiationExpression e => TransformObjectInstantiationExpression(e, slotMapBuilder),
            NewDynamicExpression e => TransformNewDynamicExpression(e, slotMapBuilder),
            WhileExpression e => TransformWhileExpression(e, slotMapBuilder),
            MatchExpression e => TransformMatchExpression(e, slotMapBuilder),
            InterpolatedStringExpression e => TransformInterpolatedStringExpression(e, slotMapBuilder),
            ForExpression e => TransformForExpression(e, slotMapBuilder),
            ReturnExpression e => TransformReturnExpression(e, slotMapBuilder),
            RangeExpression e => TransformRangeExpression(e, slotMapBuilder),
            InplaceIncrementExpression e => TransformInplaceIncrementExpression(e, slotMapBuilder),
            InplaceDecrementExpression e => TransformInplaceDecrementExpression(e, slotMapBuilder),
            VariantExpression e => TransformVariantExpression(e, slotMapBuilder),
            IIdentifierLiteral e => TransformIdentifierExpression(e, slotMapBuilder),
            _ => expr,
        };
    }

    private IIdentifierLiteral TransformIdentifierExpression(
        IIdentifierLiteral ident,
        SlotMapBuilder slotMapBuilder)
    {
        return ident switch
        {
            IdentifierLiteral l when slotMapBuilder.TryGetLocal(l.Name, out var local) =>
                TransformIdentifierToLocalGet(l, local),
            // Simple type name using simple cache (unambiguous)
            IdentifierLiteral l when program.SimpleTypeCache.TryGetValue(l.Name, out var type) =>
                AddTypeAndTransformIdentifier(l, type, slotMapBuilder),
            KeywordLiteral { Name: "self" } k when slotMapBuilder.TryGetLocal("self", out var local) =>
                TransformIdentifierToLocalGet(k, local),
            GenericNameLiteral => throw new NotImplementedException(),
            _ => throw new JitzuException(ident.Location, $"Unable to bind identifier '{ident.Name}'"),
        };
    }

    private Expression CreateGetExpression(Local local, IIdentifierLiteral identifier)
    {
        return local.LocalKind switch
        {
            LocalKind.Local => new LocalGetExpression(identifier)
            {
                SlotIndex = local.SlotIndex,
                Location = identifier.Location
            },
            LocalKind.Global => new GlobalGetExpression(identifier)
            {
                SlotIndex = local.SlotIndex,
                Location = identifier.Location,
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Transforms a let expression to use LocalSet.
    /// </summary>
    private Expression TransformLetExpression(
        LetExpression letExpr,
        SlotMapBuilder slotMapBuilder)
    {
        // Always add a new local in this case for var shadowing.
        var local = slotMapBuilder.Add(letExpr.Identifier.Name);
        return CreateSetExpression(local, letExpr.Identifier, letExpr.Value, slotMapBuilder);
    }

    private Expression CreateSetExpression(
        Local local,
        IIdentifierLiteral identifierLiteral,
        Expression value,
        SlotMapBuilder slotMapBuilder)
    {
        return local.LocalKind switch
        {
            LocalKind.Local => new LocalSetExpression
            {
                SlotIndex = local.SlotIndex,
                ValueExpression = TransformExpression(value, slotMapBuilder),
                Location = identifierLiteral.Location,
                Identifier = identifierLiteral,
            },
            LocalKind.Global => new GlobalSetExpression(identifierLiteral)
            {
                SlotIndex = local.SlotIndex,
                ValueExpression = TransformExpression(value, slotMapBuilder),
                Location = identifierLiteral.Location,
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Transforms an assignment expression to use LocalSet if the target is a local variable.
    /// </summary>
    private Expression TransformAssignmentExpression(
        AssignmentExpression assignExpr,
        SlotMapBuilder slotMapBuilder)
    {
        if (assignExpr.Left is IIdentifierLiteral ident && slotMapBuilder.TryGetLocal(ident.Name, out var local))
        {
            return local.LocalKind switch
            {
                LocalKind.Local => new LocalSetExpression
                {
                    SlotIndex = local.SlotIndex,
                    ValueExpression = TransformExpression(assignExpr.Right, slotMapBuilder),
                    Location = assignExpr.Location,
                    Identifier = ident,
                },
                LocalKind.Global => new GlobalSetExpression(ident)
                {
                    SlotIndex = local.SlotIndex,
                    ValueExpression = TransformExpression(assignExpr.Right, slotMapBuilder),
                    Location = assignExpr.Location,
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        // For non-local assignments, transform children but keep the structure
        return new AssignmentExpression
        {
            Left = TransformExpression(assignExpr.Left, slotMapBuilder),
            Right = TransformExpression(assignExpr.Right, slotMapBuilder),
            Location = assignExpr.Location
        };
    }

    /// <summary>
    /// Transforms an identifier literal to use LocalGet if it's a local variable.
    /// </summary>
    private SimpleMemberAccessExpression TransformSimpleMemberAccess(
        SimpleMemberAccessExpression ident,
        SlotMapBuilder slotMapBuilder)
    {
        ident.Object = TransformExpression(ident.Object, slotMapBuilder);
        return ident;
    }

    /// <summary>
    /// Transforms an identifier literal to use LocalGet if it's a local variable.
    /// </summary>
    private IIdentifierLiteral TransformIdentifierToLocalGet(IIdentifierLiteral ident, Local local)
    {
        return local.LocalKind switch
        {
            LocalKind.Local => new LocalGetExpression(ident)
            {
                SlotIndex = local.SlotIndex,
                Location = ident.Location,
            },
            LocalKind.Global => new GlobalGetExpression(ident)
            {
                SlotIndex = local.SlotIndex,
                Location = ident.Location,
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IIdentifierLiteral AddTypeAndTransformIdentifier(
        IdentifierLiteral ident,
        Type type,
        SlotMapBuilder slotMapBuilder)
    {
        var local = slotMapBuilder.Add(type.Name);
        return TransformIdentifierToLocalGet(ident, local);
    }

    private bool TryTransformQualifiedTypeName(
        SimpleMemberAccessExpression expr,
        SlotMapBuilder slotMapBuilder,
        out Expression result)
    {
        result = expr;

        // Try to flatten the member access expression into a qualified name
        if (FlattenMemberAccessToQualifiedName(expr, out var qualifiedName))
        {
            // Check if this is a type
            if (program.Types.TryGetValue(qualifiedName, out var type))
            {
                // Create a synthetic identifier for the type
                var syntheticIdent = new IdentifierLiteral
                {
                    Name = qualifiedName,
                    Location = expr.Location
                };

                result = AddTypeAndTransformIdentifier(syntheticIdent, type, slotMapBuilder);
                return true;
            }
        }

        return false;
    }

    private bool FlattenMemberAccessToQualifiedName(Expression expr, out string qualifiedName)
    {
        qualifiedName = "";

        if (expr is SimpleMemberAccessExpression sma)
        {
            var parts = new List<string>();
            var current = (Expression)sma;

            while (current is SimpleMemberAccessExpression memberAccess)
            {
                if (memberAccess.Property is IdentifierLiteral prop)
                {
                    parts.Insert(0, prop.Name);
                    current = memberAccess.Object;
                }
                else
                {
                    return false;
                }
            }

            if (current is IdentifierLiteral id)
            {
                parts.Insert(0, id.Name);
                qualifiedName = string.Join(".", parts);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Transforms a block body expression by transforming all its child expressions.
    /// </summary>
    private BlockBodyExpression TransformBlockBody(
        BlockBodyExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Scope = slotMapBuilder.PushScope();
        for (var index = 0; index < expression.Expressions.Length; index++)
        {
            var expr = expression.Expressions[index];
            expression.Expressions[index] = TransformExpression(expr, slotMapBuilder);
        }

        slotMapBuilder.PopScope();
        return expression;
    }

    /// <summary>
    /// Transforms a binary expression by transforming its left and right operands.
    /// </summary>
    private BinaryExpression TransformBinaryExpression(
        BinaryExpression binExpr,
        SlotMapBuilder slotMapBuilder)
    {
        binExpr.Left = TransformExpression(binExpr.Left, slotMapBuilder);
        binExpr.Right = TransformExpression(binExpr.Right, slotMapBuilder);
        return binExpr;
    }

    private IfExpression TransformIfExpression(
        IfExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Condition = TransformExpression(expression.Condition, slotMapBuilder);
        expression.Then = TransformExpression(expression.Then, slotMapBuilder);
        expression.Else = expression.Else is not null
            ? TransformExpression(expression.Else, slotMapBuilder)
            : null;

        return expression;
    }

    private IndexerExpression TransformIndexerExpression(
        IndexerExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Identifier = TransformExpression(expression.Identifier, slotMapBuilder);
        expression.Index = TransformExpression(expression.Index, slotMapBuilder);
        return expression;
    }

    private TryExpression TransformTryExpression(
        TryExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Body = TransformExpression(expression.Body, slotMapBuilder);
        return expression;
    }

    private ObjectInstantiationExpression TransformObjectInstantiationExpression(
        ObjectInstantiationExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Identifier = (IIdentifierLiteral)TransformExpression(expression.Identifier, slotMapBuilder);
        expression.Body = (NewDynamicExpression)TransformExpression(expression.Body, slotMapBuilder);
        return expression;
    }

    private NewDynamicExpression TransformNewDynamicExpression(
        NewDynamicExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        foreach (var field in expression.Fields)
        {
            field.Value = field.Value is not null
                ? TransformExpression(field.Value, slotMapBuilder)
                : TransformExpression(field.Identifier, slotMapBuilder);
        }

        return expression;
    }

    private WhileExpression TransformWhileExpression(
        WhileExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Condition = TransformExpression(expression.Condition, slotMapBuilder);
        for (var i = 0; i < expression.Body.Length; i++)
            expression.Body[i] = TransformExpression(expression.Body[i], slotMapBuilder);
        return expression;
    }

    private BlockBodyExpression TransformMatchExpression(
        MatchExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        var identifier = new IdentifierLiteral
        {
            Name = Guid.NewGuid().ToString(),
            Location = expression.Expression.Location,
        };

        var local = slotMapBuilder.Add(identifier.Name);
        var tempExpression = CreateSetExpression(local, identifier, expression.Expression, slotMapBuilder);

        expression.Expression = CreateGetExpression(local, identifier);
        foreach (var matchCase in expression.Cases)
        {
            slotMapBuilder.PushScope();
            matchCase.Pattern = TransformExpression(matchCase.Pattern, slotMapBuilder);
            matchCase.Body = TransformExpression(matchCase.Body, slotMapBuilder);
            slotMapBuilder.PopScope();
        }

        return new BlockBodyExpression
        {
            Expressions = [tempExpression, expression]
        };
    }

    private InterpolatedStringExpression TransformInterpolatedStringExpression(
        InterpolatedStringExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        foreach (var part in expression.Parts)
            part.Expression = TransformExpression(part.Expression, slotMapBuilder);

        return expression;
    }

    private ForExpression TransformForExpression(ForExpression expression, SlotMapBuilder slotMapBuilder)
    {
        // After we have added the local, we dont need to change anything about the identifier 
        slotMapBuilder.Add(expression.Identifier.Name);
        expression.Range = TransformExpression(expression.Range, slotMapBuilder);
        expression.Body = TransformBlockBody(expression.Body, slotMapBuilder);
        return expression;
    }

    private ReturnExpression TransformReturnExpression(
        ReturnExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        if (expression.ReturnValue is { } returnValue)
            expression.ReturnValue = TransformExpression(returnValue, slotMapBuilder);
        return expression;
    }

    private RangeExpression TransformRangeExpression(
        RangeExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        if (expression.Left is not null)
            expression.Left = TransformExpression(expression.Left, slotMapBuilder);

        if (expression.Right is not null)
            expression.Right = TransformExpression(expression.Right, slotMapBuilder);

        return expression;
    }

    private InplaceIncrementExpression TransformInplaceIncrementExpression(
        InplaceIncrementExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Subject = TransformExpression(expression.Subject, slotMapBuilder);
        return expression;
    }

    private InplaceDecrementExpression TransformInplaceDecrementExpression(
        InplaceDecrementExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Subject = TransformExpression(expression.Subject, slotMapBuilder);
        return expression;
    }

    /// <summary>
    /// Transforms a function call expression by transforming its identifier and arguments.
    /// </summary>
    private FunctionCallExpression TransformFunctionCallExpression(
        FunctionCallExpression expression,
        SlotMapBuilder slotMapBuilder)
    {
        expression.Identifier = TransformExpression(expression.Identifier, slotMapBuilder);

        for (int i = 0; i < expression.Arguments.Length; i++)
        {
            var arg = expression.Arguments[i];
            expression.Arguments[i] = TransformExpression(arg, slotMapBuilder);
        }

        return expression;
    }

    private VariantExpression TransformVariantExpression(
        VariantExpression variantExpression,
        SlotMapBuilder slotMapBuilder)
    {
        variantExpression.Identifier = TransformIdentifierExpression(variantExpression.Identifier, slotMapBuilder);
        if (variantExpression.PositionalPattern != null)
        {
            var parts = variantExpression.PositionalPattern.Parts;
            for (var i = 0; i < variantExpression.PositionalPattern.Parts.Length; i++)
            {
                switch (parts[i])
                {
                    case IdentifierLiteral identifier:
                        var local = slotMapBuilder.Add(identifier.Name);
                        parts[i] = CreateGetExpression(local, identifier);
                        break;
                }
            }
        }

        return variantExpression;
    }
}