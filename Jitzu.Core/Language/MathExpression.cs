namespace Jitzu.Core.Language;

public static class MathExpression
{
    public static Expression? Compute(string token, Expression left, Expression right)
    {
        return left switch
        {
            IntLiteral i => ResolveIntExpression(token, i, right),
            DoubleLiteral d => ResolveDoubleExpression(token, d, right),
            _ => null
        };
    }

    private static Expression? ResolveIntExpression(string token, IntLiteral left, Expression right)
    {
        return token switch
        {
            "+" => right switch
            {
                IntLiteral i => new IntLiteral
                {
                    Integer = left.Integer + i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Integer + d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "-" => right switch
            {
                IntLiteral i => new IntLiteral
                {
                    Integer = left.Integer - i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Integer - d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "*" => right switch
            {
                IntLiteral i => new IntLiteral
                {
                    Integer = left.Integer * i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Integer * d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "/" => right switch
            {
                IntLiteral i => new IntLiteral
                {
                    Integer = left.Integer / i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Integer / d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "%" => right switch
            {
                IntLiteral i => new IntLiteral
                {
                    Integer = left.Integer % i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Integer % d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            _ => null
        };
    }
    
    private static Expression? ResolveDoubleExpression(string token, DoubleLiteral left, Expression right)
    {
        return token switch
        {
            "+" => right switch
            {
                IntLiteral i => new DoubleLiteral
                {
                    Double = left.Double + i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Double + d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "-" => right switch
            {
                IntLiteral i => new DoubleLiteral
                {
                    Double = left.Double - i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Double - d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "*" => right switch
            {
                IntLiteral i => new DoubleLiteral
                {
                    Double = left.Double * i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Double * d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "/" => right switch
            {
                IntLiteral i => new DoubleLiteral
                {
                    Double = left.Double / i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Double / d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            "%" => right switch
            {
                IntLiteral i => new DoubleLiteral
                {
                    Double = left.Double % i.Integer,
                    Location = left.Location.Extend(right.Location),
                },
                DoubleLiteral d => new DoubleLiteral
                {
                    Double = left.Double % d.Double,
                    Location = left.Location.Extend(right.Location),
                },
                _ => null,
            },
            _ => null
        };
    }
}