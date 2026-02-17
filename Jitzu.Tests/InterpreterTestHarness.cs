using Jitzu.Core;
using Jitzu.Core.Language;
using Jitzu.Core.Runtime;
using Jitzu.Core.Runtime.Compilation;

namespace Jitzu.Tests;

/// <summary>
/// Runs Jitzu source code through the full compilation and execution pipeline,
/// capturing console output for assertions.
/// </summary>
public static class InterpreterTestHarness
{
    private static readonly Lock ConsoleOutputLock = new();

    /// <summary>
    /// Executes a Jitzu source code string and returns the captured console output.
    /// </summary>
    public static async Task<string> RunAsync(string sourceCode, string[]? args = null)
    {
        var ast = new ScriptExpression
        {
            Body = Parser.Parse("", sourceCode)
        };

        var program = await ProgramBuilder.Build(ast);
        var analyser = new SemanticAnalyser(program);
        ast = analyser.AnalyseScript(ast);

        var script = new ByteCodeCompiler(program).Compile(ast.Body);

        lock (ConsoleOutputLock)
        {
            var oldOut = Console.Out;
            using var writer = new StringWriter();
            Console.SetOut(writer);
            try
            {
                var interpreter = new ByteCodeInterpreter(program, script, args ?? [], false);
                interpreter.Evaluate();
            }
            finally
            {
                Console.SetOut(oldOut);
            }

            return writer.ToString().TrimEnd();
        }
    }
}
