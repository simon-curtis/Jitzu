using Shouldly;

namespace Jitzu.Tests;

public class ClosureTests
{
    [Test]
    public async Task NestedFunction_CapturesParameter()
    {
        const string source = """
                              fun make_adder(x: Int): Int {
                                  fun add(y: Int): Int {
                                      return x + y
                                  }
                                  return add(5)
                              }

                              print(make_adder(10))
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("15");
    }

    [Test]
    public async Task NestedFunction_MutatesOuterVariable()
    {
        const string source = """
                              fun test(): Int {
                                  let mut count = 0
                                  fun increment(n: Int) {
                                      count = count + n
                                  }
                                  increment(5)
                                  increment(3)
                                  return count
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("8");
    }

    [Test]
    public async Task NestedFunction_MutationAccumulates()
    {
        const string source = """
                              fun test(): Int {
                                  let mut total = 0
                                  fun add(n: Int): Int {
                                      total = total + n
                                      return total
                                  }
                                  add(10)
                                  add(20)
                                  return total
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("30");
    }

    [Test]
    public async Task NestedFunction_CapturesMultipleVariables()
    {
        const string source = """
                              fun test(): Int {
                                  let a = 10
                                  let b = 20
                                  fun sum(): Int {
                                      return a + b
                                  }
                                  return sum()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("30");
    }

    [Test]
    public async Task NestedFunction_ShadowsOuterVariable()
    {
        const string source = """
                              fun test(): Int {
                                  let x = 10
                                  fun inner(): Int {
                                      let x = 99
                                      return x
                                  }
                                  return inner()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("99");
    }

    [Test]
    public async Task NestedFunction_OuterVariableUnchangedAfterShadow()
    {
        const string source = """
                              fun test(): Int {
                                  let x = 10
                                  fun inner(): Int {
                                      let x = 99
                                      return x
                                  }
                                  inner()
                                  return x
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("10");
    }

    [Test]
    public async Task DeeplyNestedFunction_CapturesFromGrandparent()
    {
        const string source = """
                              fun test(): Int {
                                  let x = 42
                                  fun middle(): Int {
                                      fun inner(): Int {
                                          return x
                                      }
                                      return inner()
                                  }
                                  return middle()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("42");
    }

    [Test]
    public async Task TwoNestedFunctions_ShareCapturedVariable()
    {
        const string source = """
                              fun test(): Int {
                                  let mut x = 0
                                  fun inc() {
                                      x = x + 1
                                  }
                                  fun get(): Int {
                                      return x
                                  }
                                  inc()
                                  inc()
                                  inc()
                                  return get()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("3");
    }

    [Test]
    public async Task NestedFunction_CapturesGlobalVariable()
    {
        const string source = """
                              let x = 100

                              fun test(): Int {
                                  fun inner(): Int {
                                      return x
                                  }
                                  return inner()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("100");
    }

    [Test]
    public async Task NestedFunction_CaptureAndLocalCoexist()
    {
        const string source = """
                              fun test(): Int {
                                  let captured = 10
                                  fun inner(): Int {
                                      let local = 5
                                      return captured + local
                                  }
                                  return inner()
                              }

                              print(test())
                              """;

        var output = await InterpreterTestHarness.RunAsync(source);
        output.ShouldBe("15");
    }
}
