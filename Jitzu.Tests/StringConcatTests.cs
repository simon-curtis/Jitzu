using Jitzu.Core.Runtime;
using Shouldly;

namespace Jitzu.Tests;

public class StringConcatTests
{
    [Test]
    public async Task PlusOperator_StringPlusString_ConcatenatesAtRuntime()
    {
        const string source = """
                              print("hello " + "world")
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("hello world");
    }

    [Test]
    public async Task PlusOperator_StringPlusInt_StringifiesAndConcatenates()
    {
        const string source = """
                              let x = 42
                              print("x = " + x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("x = 42");
    }

    [Test]
    public async Task PlusOperator_IntPlusString_StringifiesAndConcatenates()
    {
        const string source = """
                              let x = 42
                              print(x + " = answer")
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("42 = answer");
    }

    [Test]
    public async Task PlusOperator_StringPlusBool_StringifiesAndConcatenates()
    {
        const string source = """
                              print("flag = " + true)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("flag = True");
    }

    [Test]
    public void Add_StringPlusString_ReturnsConcatenatedRef()
    {
        var result = BinaryExpressionEvaluator.Add(
            Value.FromRef("foo"),
            Value.FromRef("bar"));

        result.Kind.ShouldBe(ValueKind.Ref);
        result.Ref.ShouldBe("foobar");
    }

    [Test]
    public void Add_StringPlusInt_StringifiesRhs()
    {
        var result = BinaryExpressionEvaluator.Add(
            Value.FromRef("x = "),
            Value.FromInt(42));

        result.Kind.ShouldBe(ValueKind.Ref);
        result.Ref.ShouldBe("x = 42");
    }

    [Test]
    public void Add_IntPlusString_StringifiesLhs()
    {
        var result = BinaryExpressionEvaluator.Add(
            Value.FromInt(42),
            Value.FromRef(" = x"));

        result.Kind.ShouldBe(ValueKind.Ref);
        result.Ref.ShouldBe("42 = x");
    }

    [Test]
    public void Add_StringPlusDouble_UsesInvariantCulture()
    {
        var result = BinaryExpressionEvaluator.Add(
            Value.FromRef("pi = "),
            Value.FromDouble(3.14));

        result.Kind.ShouldBe(ValueKind.Ref);
        result.Ref.ShouldBe("pi = 3.14");
    }

    [Test]
    public void Add_StringPlusBool_StringifiesBool()
    {
        var result = BinaryExpressionEvaluator.Add(
            Value.FromRef("ok = "),
            Value.FromBool(true));

        result.Kind.ShouldBe(ValueKind.Ref);
        result.Ref.ShouldBe("ok = True");
    }

    [Test]
    public void Add_RefNonStringPlusRefNonString_StillThrows()
    {
        var lhs = Value.FromRef(new object());
        var rhs = Value.FromRef(new object());

        Should.Throw<OperationNotSupportedException>(() =>
            BinaryExpressionEvaluator.Add(lhs, rhs));
    }

    [Test]
    public void Add_StringPlusRefNonString_StillThrows()
    {
        var lhs = Value.FromRef("prefix:");
        var rhs = Value.FromRef(new object());

        Should.Throw<OperationNotSupportedException>(() =>
            BinaryExpressionEvaluator.Add(lhs, rhs));
    }
}
