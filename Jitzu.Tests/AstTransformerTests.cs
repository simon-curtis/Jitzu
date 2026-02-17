using Jitzu.Core;
using Jitzu.Core.Language;
using Jitzu.Core.Runtime;
using Jitzu.Core.Runtime.Compilation;
using Jitzu.Core.Runtime.Memory;
using Jitzu.Core.Types;
using Shouldly;

namespace Jitzu.Tests;

public class AstTransformerTests
{
    private static ScriptExpression CreateScript(ReadOnlySpan<char> sourceCode)
    {
        return new ScriptExpression
        {
            Body = Parser.Parse("", sourceCode)
        };
    }

    private static AstTransformer CreateTransformer(
        SlotMapBuilder builder,
        Dictionary<string, Type>? simpleTypeCache = null)
    {
        // Register print as a built-in function (mirrors ProgramBuilder.Build)
        builder.Add("print");

        var program = new RuntimeProgram
        {
            Types = new Dictionary<string, Type>(),
            SimpleTypeCache = simpleTypeCache ?? new Dictionary<string, Type>(),
            TypeNameConflicts = new Dictionary<string, HashSet<string>>(),
            FileNamespaces = new Dictionary<string, string>(),
            Globals = new Dictionary<string, Type>(),
            MethodTable = new MethodTable(),
            GlobalFunctions = new Dictionary<string, IShellFunction>
            {
                ["print"] = new ForeignFunction(GlobalFunctions.PrintStatic),
            },
            GlobalSlotMap = new Dictionary<string, int>(),
            SlotBuilder = builder,
        };
        return new AstTransformer(program);
    }

    [Test]
    public void GlobalVariables_AreMappedCorrect()
    {
        const string sourceCode = """
                                  let x = 0;
                                  let y = 0;
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var scope = builder.PushScope();

        var ast = CreateScript(sourceCode);
        CreateTransformer(builder).TransformScriptExpression(ast, builder);

        // print takes slot 0, x=1, y=2
        ast.Body[0].ShouldBeOfType<GlobalSetExpression>().SlotIndex.ShouldBe(1);
        ast.Body[1].ShouldBeOfType<GlobalSetExpression>().SlotIndex.ShouldBe(2);

        scope["x"].ShouldBe(1);
        scope["y"].ShouldBe(2);
    }

    [Test]
    public void LocalVariables_AreMappedCorrect()
    {
        const string sourceCode = """
                                  fun foo() {
                                      let x = 0
                                      let y = 0
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        builder.PushScope();

        var ast = CreateScript(sourceCode);
        var function = ast.Body.First().ShouldBeOfType<FunctionDefinitionExpression>();
        var (scope, _) = CreateTransformer(builder).TransformFunctionBody(function, builder);

        // Local slots are independent of global slots
        function.Body[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);
        function.Body[1].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(1);

        scope["x"].ShouldBe(0);
        scope["y"].ShouldBe(1);
    }

    [Test]
    public void GlobalVariables_AreMappedCorrect_WhenInsideFunction()
    {
        const string sourceCode = """
                                  let x = 0
                                  let y = 0

                                  fun foo() {
                                    print(x)
                                    print(y)
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var globalScope = builder.PushScope();
        var transformer = CreateTransformer(builder);

        var ast = CreateScript(sourceCode);
        transformer.TransformScriptExpression(ast, builder);

        // print=0, x=1, y=2
        globalScope["x"].ShouldBe(1);
        globalScope["y"].ShouldBe(2);

        var function = ast.Body[2].ShouldBeOfType<FunctionDefinitionExpression>();
        transformer.TransformFunctionBody(function, builder);

        function.Body[0].ShouldBeOfType<FunctionCallExpression>().Arguments[0].ShouldBeOfType<GlobalGetExpression>().SlotIndex.ShouldBe(1);
        function.Body[1].ShouldBeOfType<FunctionCallExpression>().Arguments[0].ShouldBeOfType<GlobalGetExpression>().SlotIndex.ShouldBe(2);
    }

    [Test]
    public void Locals_AreMappedCorrect_WhenFunctionShadowsParent()
    {
        const string sourceCode = """
                                  let x = 0

                                  fun foo() {
                                    let x = 1
                                    print(x)
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var globalScope = builder.PushScope();
        var transformer = CreateTransformer(builder);

        var ast = CreateScript(sourceCode);
        transformer.TransformScriptExpression(ast, builder);
        // print=0, x=1
        globalScope["x"].ShouldBe(1);

        var function = ast.Body[1].ShouldBeOfType<FunctionDefinitionExpression>();
        var (scope, _) = transformer.TransformFunctionBody(function, builder);

        function.Body[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);
        function.Body[1].ShouldBeOfType<FunctionCallExpression>().Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(0);

        scope["x"].ShouldBe(0);
    }


    [Test]
    public void FunctionParameters_AreMappedCorrect()
    {
        const string sourceCode = """
                                  fun foo(a: Int, b: Int) {
                                      print(a)
                                      print(b)
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        builder.PushScope();

        var ast = CreateScript(sourceCode);
        var function = ast.Body.First().ShouldBeOfType<FunctionDefinitionExpression>();
        var (scope, _) = CreateTransformer(builder).TransformFunctionBody(function, builder);

        // Parameters should be mapped first
        scope["a"].ShouldBe(0);
        scope["b"].ShouldBe(1);

        function.Body[0].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(0);
        function.Body[1].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(1);
    }

    [Test]
    public void NestedFunctions_CanAccessOuterVariables()
    {
        const string sourceCode = """
                                  let x = 42

                                  fun outer() {
                                      fun inner() {
                                          print(x)
                                      }
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var globalScope = builder.PushScope();
        var transformer = CreateTransformer(builder);

        var ast = CreateScript(sourceCode);
        transformer.TransformScriptExpression(ast, builder);

        // print=0, x=1
        globalScope["x"].ShouldBe(1);

        var outer = ast.Body[1].ShouldBeOfType<FunctionDefinitionExpression>();
        transformer.TransformFunctionBody(outer, builder);

        // Inner function is automatically transformed by TransformFunctionBody(outer)
        // Nested functions are wrapped in LocalSetExpression for local storage
        var innerSet = outer.Body[0].ShouldBeOfType<LocalSetExpression>();
        var inner = innerSet.ValueExpression.ShouldBeOfType<FunctionDefinitionExpression>();

        // Inner function should reference global x
        inner.Body[0].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<GlobalGetExpression>().SlotIndex.ShouldBe(1);
    }

    [Test]
    public void BlockScopes_AreMappedCorrect()
    {
        const string sourceCode = """
                                  fun foo() {
                                      {
                                          let x = 1
                                          print(x)
                                      }
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        builder.PushScope();

        var ast = CreateScript(sourceCode);
        var function = ast.Body.First().ShouldBeOfType<FunctionDefinitionExpression>();
        CreateTransformer(builder).TransformFunctionBody(function, builder);

        var block = function.Body[0].ShouldBeOfType<BlockBodyExpression>();

        block.Expressions[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);
        block.Expressions[1].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(0);

        block.Scope["x"].ShouldBe(0);
    }

    [Test]
    public void Shadowing_WorksAcrossNestedScopes()
    {
        const string sourceCode = """
                                  let x = 0

                                  fun foo() {
                                      let x = 1
                                      {
                                          let x = 2
                                          print(x)
                                      }
                                      print(x)
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var globalScope = builder.PushScope();
        var transformer = CreateTransformer(builder);

        var ast = CreateScript(sourceCode);
        transformer.TransformScriptExpression(ast, builder);

        // print=0, x=1
        globalScope["x"].ShouldBe(1);

        var function = ast.Body[1].ShouldBeOfType<FunctionDefinitionExpression>();
        var (scope, _) = transformer.TransformFunctionBody(function, builder);

        // First local x
        function.Body[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);

        // Block-scoped x
        var block = function.Body[1].ShouldBeOfType<BlockBodyExpression>();
        block.Expressions[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(1);
        block.Expressions[1].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(1);

        // After block, should still reference function-level x
        function.Body[2].ShouldBeOfType<FunctionCallExpression>()
            .Arguments[0].ShouldBeOfType<LocalGetExpression>().SlotIndex.ShouldBe(0);

        scope["x"].ShouldBe(0);
        block.Scope["x"].ShouldBe(1);
    }

    [Test]
    public void MultipleFunctions_HaveIndependentScopes()
    {
        const string sourceCode = """
                                  fun foo() {
                                      let x = 1
                                  }

                                  fun bar() {
                                      let x = 2
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        builder.PushScope();
        var transformer = CreateTransformer(builder);

        var ast = CreateScript(sourceCode);

        var foo = ast.Body[0].ShouldBeOfType<FunctionDefinitionExpression>();
        var (fooScope, _) = transformer.TransformFunctionBody(foo, builder);
        foo.Body[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);
        fooScope["x"].ShouldBe(0);

        var bar = ast.Body[1].ShouldBeOfType<FunctionDefinitionExpression>();
        var (barScope, _) = transformer.TransformFunctionBody(bar, builder);
        bar.Body[0].ShouldBeOfType<LocalSetExpression>().SlotIndex.ShouldBe(0);
        barScope["x"].ShouldBe(0);
    }

    [Test]
    public void MatchExpression_MapsLocalVariables()
    {
        const string sourceCode = """
                                  let x = Ok(1)

                                  match x {
                                      Ok(i) => true,
                                      _ => false,
                                  }
                                  """;

        var builder = new SlotMapBuilder(null, LocalKind.Global);
        var scope = builder.PushScope();

        var simpleTypeCache = new Dictionary<string, Type>
        {
            ["Ok"] = typeof(Ok<>),
        };
        var transformer = CreateTransformer(builder, simpleTypeCache);

        var ast = CreateScript(sourceCode);
        transformer.TransformScriptExpression(ast, builder);

        // print=0, Ok=1, x=2
        // Match is transformed into a BlockBodyExpression containing [tempSet, matchExpr]
        var matchBlock = ast.Body[1].ShouldBeOfType<BlockBodyExpression>();
        var matchExpression = matchBlock.Expressions[1].ShouldBeOfType<MatchExpression>();
        var matchCondition = matchExpression.Expression.ShouldBeOfType<GlobalGetExpression>();

        var firstCase = matchExpression.Cases[0].ShouldBeOfType<MatchArm>();
        var variantPattern = firstCase.Pattern.ShouldBeOfType<VariantExpression>();
        var partExpression = variantPattern.PositionalPattern!.Parts[0].ShouldBeOfType<GlobalGetExpression>();
        partExpression.Identifier.ShouldBeOfType<IdentifierLiteral>().Name.ShouldBe("i");
    }
}
