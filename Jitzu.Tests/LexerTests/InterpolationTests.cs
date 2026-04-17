using Jitzu.Core;
using Shouldly;

namespace Jitzu.Tests.LexerTests;

public class InterpolationTests
{
    [Test]
    public void DollarBrace_InBacktickString_ThrowsWithHelpfulMessage()
    {
        JitzuException? caught = null;
        try
        {
            var lexer = new Lexer("", "`Hello, ${name}`");
            lexer.Lex();
        }
        catch (JitzuException ex)
        {
            caught = ex;
        }

        caught.ShouldNotBeNull();
        caught.Message.ShouldContain("{name}");
        caught.Message.ShouldContain("{expr}");
    }

    [Test]
    public void PlainBrace_InBacktickString_DoesNotThrow()
    {
        var lexer = new Lexer("", "`Hello, {name}`");
        var tokens = lexer.Lex();
        tokens.ShouldNotBeEmpty();
    }

    [Test]
    public void LiteralDollar_NotFollowedByBrace_DoesNotThrow()
    {
        var lexer = new Lexer("", "`Price: $5`");
        var tokens = lexer.Lex();
        tokens.ShouldNotBeEmpty();
    }
}
