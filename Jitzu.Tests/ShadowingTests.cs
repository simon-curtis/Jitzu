using Shouldly;

namespace Jitzu.Tests;

public class ShadowingTests
{
    [Test]
    public async Task BlockScope_ShadowsOuterVariable()
    {
        const string source = """
                              let x = 1
                              {
                                  let x = 2
                                  print(x)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("2");
    }

    [Test]
    public async Task BlockScope_OuterVariableUnchangedAfterShadow()
    {
        const string source = """
                              let x = 1
                              {
                                  let x = 2
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("1");
    }

    [Test]
    public async Task BlockScope_BothValuesAccessible()
    {
        const string source = """
                              let x = 1
                              {
                                  let x = 2
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("2\n1");
    }

    [Test]
    public async Task NestedBlocks_MultipleShadowLevels()
    {
        const string source = """
                              let x = 1
                              {
                                  let x = 2
                                  {
                                      let x = 3
                                      print(x)
                                  }
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("3\n2\n1");
    }

    [Test]
    public async Task FunctionParameter_ShadowsGlobal()
    {
        const string source = """
                              let x = 10
                              fun test(x: Int): Int {
                                  return x
                              }
                              print(test(42))
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("42\n10");
    }

    [Test]
    public async Task FunctionLocal_ShadowsGlobal()
    {
        const string source = """
                              let x = 10
                              fun test(): Int {
                                  let x = 99
                                  return x
                              }
                              print(test())
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("99\n10");
    }

    [Test]
    public async Task IfBlock_ShadowsOuterVariable()
    {
        const string source = """
                              let x = 1
                              if true {
                                  let x = 2
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("2\n1");
    }

    [Test]
    public async Task IfBlock_ShadowDoesNotLeakAfterBlock()
    {
        const string source = """
                              let x = 1
                              if true {
                                  let x = 2
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("1");
    }

    [Test]
    public async Task IfElse_BothBranchesShadowIndependently()
    {
        const string source = """
                              let x = 1
                              if false {
                                  let x = 2
                                  print(x)
                              } else {
                                  let x = 3
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("3\n1");
    }

    [Test]
    public async Task WhileLoop_ShadowsOuterVariable()
    {
        const string source = """
                              let x = 100
                              let mut i = 0
                              while i < 1 {
                                  let x = 42
                                  print(x)
                                  i = i + 1
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("42\n100");
    }

    [Test]
    public async Task ForLoop_IteratorShadowsOuter()
    {
        const string source = """
                              let i = 999
                              for i in 0..3 {
                                  print(i)
                              }
                              print(i)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("0\n1\n2\n999");
    }

    [Test]
    public async Task SameScope_Redeclaration()
    {
        const string source = """
                              let x = 1
                              print(x)
                              let x = 2
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("1\n2");
    }

    [Test]
    public async Task WhileLoop_ShadowDoesNotLeakAfterLoop()
    {
        const string source = """
                              let x = 100
                              let mut i = 0
                              while i < 1 {
                                  let x = 42
                                  i = i + 1
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("100");
    }

    [Test]
    public async Task WhileLoop_PrintAfterLoopExecutes()
    {
        const string source = """
                              let mut i = 0
                              while i < 1 {
                                  i = i + 1
                              }
                              print("after")
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("after");
    }

    [Test]
    public async Task Shadow_DifferentTypes()
    {
        const string source = """
                              let x = 42
                              {
                                  let x = "hello"
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("hello\n42");
    }

    [Test]
    public async Task Shadow_MutableOuterImmutableInner()
    {
        const string source = """
                              let mut x = 1
                              x = 5
                              {
                                  let x = 99
                                  print(x)
                              }
                              print(x)
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("99\n5");
    }
}
