using Jitzu.Core;
using Jitzu.Core.Language;

namespace Jitzu.Tests.LexerTests;

public class OperatorTests
{
    [Test]
    public void ArrowTest()
    {
        var lexer = new Lexer("", "=>");
        var tokens = lexer.Lex();
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Operator));
        Assert.That(tokens[0].Value, Is.EqualTo("=>"));
    }
}