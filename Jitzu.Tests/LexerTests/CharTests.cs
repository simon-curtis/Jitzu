using Jitzu.Core;
using Jitzu.Core.Language;
using Shouldly;

namespace Jitzu.Tests.LexerTests;

public class CharTests
{
    [Test]
    public async Task EmptyCharLiteral_ShouldThrow()
    {
        await Assert.ThrowsAsync<JitzuException>(() =>
        {
            var lexer = new Lexer("", "  ''");
            lexer.Lex();
            return Task.CompletedTask;
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
    public async Task InvalidEscapeChar_ShouldThrow()
    {
        await Assert.ThrowsAsync<JitzuException>(() =>
        {
            var lexer = new Lexer("", "  '\\m'");
            lexer.Lex();
            return Task.CompletedTask;
        });
    }
}