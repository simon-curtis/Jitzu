using Jitzu.Core;
using Jitzu.Core.Language;
using Shouldly;

namespace Jitzu.Tests.LexerTests;

public class OperatorTests
{
    [Test]
    public void ArrowTest()
    {
        var lexer = new Lexer("", "=>");
        var tokens = lexer.Lex();
        tokens[0].Type.ShouldBe(TokenType.Operator);
        tokens[0].Value.ShouldBe("=>");
    }
}