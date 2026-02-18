using Jitzu.Core;
using Shouldly;

namespace Jitzu.Tests;

public class ForLoopTests
{
    [Test]
    public async Task ForLoop_OverIntRange()
    {
        const string source = """
                              for i in 0..3 {
                                  print(i)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("0\n1\n2");
    }

    [Test]
    public async Task ForLoop_OverArray()
    {
        const string source = """
                              let items = [1, 2, 3]
                              for item in items {
                                  print(item)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("1\n2\n3");
    }

    [Test]
    public async Task ForLoop_OverStringArray()
    {
        const string source = """
                              let names = ["alice", "bob", "charlie"]
                              for name in names {
                                  print(name)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("alice\nbob\ncharlie");
    }

    [Test]
    public async Task ForLoop_OverEmptyArray()
    {
        const string source = """
                              let items = []
                              for item in items {
                                  print(item)
                              }
                              print("done")
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("done");
    }

    [Test]
    public async Task ForLoop_OverString()
    {
        const string source = """
                              for ch in "abc" {
                                  print(ch)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("a\nb\nc");
    }

    [Test]
    public async Task ForLoop_CannotMutateCollection()
    {
        const string source = """
                              let items = [1, 2, 3]
                              for item in items {
                                  items = [4, 5, 6]
                              }
                              """;

        var ex = await Assert.ThrowsAsync<JitzuException>(async () => await InterpreterTestHarness.RunAsync(source));
        ex.Message.ShouldContain("Cannot assign to 'items' while iterating over it");
    }

    [Test]
    public async Task ForLoop_ShadowingCollectionVariableIsAllowed()
    {
        const string source = """
                              let items = [1, 2, 3]
                              for item in items {
                                  let items = 99
                                  print(items)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("99\n99\n99");
    }

    [Test]
    public async Task ForLoop_CannotMutateCollectionInNestedIf()
    {
        const string source = """
                              let items = [1, 2, 3]
                              for item in items {
                                  if true {
                                      items = []
                                  }
                              }
                              """;

        var ex = await Assert.ThrowsAsync<JitzuException>(async () => await InterpreterTestHarness.RunAsync(source));
        ex.Message.ShouldContain("Cannot assign to 'items' while iterating over it");
    }

    [Test]
    public async Task ForLoop_LiteralRangeAllowsSameNameVariable()
    {
        const string source = """
                              for item in [1, 2, 3] {
                                  print(item)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("1\n2\n3");
    }

    [Test]
    public async Task ForLoop_OverBoolArray()
    {
        const string source = """
                              for b in [true, false, true] {
                                  print(b)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("True\nFalse\nTrue");
    }

    [Test]
    public async Task ForLoop_OverMixedExpressionArray()
    {
        const string source = """
                              let x = 10
                              let items = [x, 20, 30]
                              for item in items {
                                  print(item)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("10\n20\n30");
    }

    [Test]
    public async Task ForLoop_OverSingleElementArray()
    {
        const string source = """
                              for item in [42] {
                                  print(item)
                              }
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("42");
    }

    // Note: EvaluateIndexGet (safe items[idx]) and EvaluateIndexSet on Value[] arrays
    // cannot be tested through the harness — the semantic analyser does not support
    // indexing on untyped list literals, and the compiler does not emit IndexSet.
    // The Value[] fast paths in those methods are defensive; the primary hot path
    // (NewList + IndexGetDirect) is exercised by the for-each tests above.
}
