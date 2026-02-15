using Jitzu.Core;
using Jitzu.Core.Language;
using Shouldly;

namespace Jitzu.Tests.LexerTests;

public class CharTests
{
    [Test]
    public void EmptyCharLiteral_ShouldThrow()
    {
        Assert.Throws<JitzuException>(() =>
        {
            var lexer = new Lexer("", "  ''");
            lexer.Lex();
        });
    }

    [Test]
    public void NormalChar_ShouldReturnSingleCharToken()
    {
        var lexer = new Lexer("", "  'c'");
        var tokens = lexer.Lex();
        tokens[0].Type.ShouldBe(TokenType.Char);
        tokens[0].Value.ShouldBe("'c'");
        tokens[0].Span.Start.Column.ShouldBe(3);
        tokens[0].Span.End.Column.ShouldBe(6);
    }

    [Test]
    public void EscapeChar_ShouldReturnSingleCharToken()
    {
        var lexer = new Lexer("", "  '\\t'");
        var tokens = lexer.Lex();
        tokens[0].Type.ShouldBe(TokenType.Char);
        tokens[0].Value.ShouldBe("'\\t'");
        tokens[0].Span.Start.Column.ShouldBe(3);
        tokens[0].Span.End.Column.ShouldBe(7);
    }
    
    [Test]
    public void InvalidEscapeChar_ShouldThrow()
    {
        Assert.Throws<JitzuException>(() =>
        {
            var lexer = new Lexer("", "  '\\m'");
            lexer.Lex();
        });
    }
}