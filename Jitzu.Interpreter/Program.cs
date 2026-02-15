using System.Diagnostics;
using System.Text;
using Jitzu.Core;
using Jitzu.Core.Common.Logging;
using Jitzu.Core.Language;
using Jitzu.Core.Logging;
using Jitzu.Core.Runtime;
using Jitzu.Core.Runtime.Compilation;
using Jitzu.Interpreter.Infrastructure.Configuration;
using Jitzu.Interpreter.Infrastructure.Logging;

ConsoleEx.ConfigureOutput();
Console.OutputEncoding = Encoding.UTF8;

var appArgs = JitzuCli.Parse(args);
try
{
    DebugLogger.SetIsEnabled(appArgs.Debug);
    Telemetry.SetIsEnabled(appArgs.Telemetry);

    return appArgs switch
    {
        { InstallPath: true } => DisplayInstallPath(),
        { Action: CliActions.RunAction runAction } => await RunScript(runAction.EntryPoint, runAction.ScriptArgs),
        { EntryPoint: not null } => await RunScript(appArgs.EntryPoint, appArgs.ScriptArgs),
        _ => DisplayHelp()
    };
}
catch (JitzuException ex)
{
    ExceptionPrinter.Print(ex);
    return 1;
}
finally
{
    Console.Out.Flush();
}

int DisplayInstallPath()
{
    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
    return 0;
}

int DisplayHelp()
{
    JitzuCli.PrintHelpMessage();
    return 0;
}

async Task<int> RunScript(string filePath, string[] args)
{
    var entryPointPath = Path.ChangeExtension(filePath, "jz");

    if (entryPointPath.StartsWith('~'))
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        entryPointPath = Path.Join(profile, entryPointPath[1..]);
    }

    if (!File.Exists(entryPointPath))
    {
        Console.WriteLine($"Entry point: {entryPointPath} does not exist");
        return 1;
    }

    var entryPoint = new FileInfo(entryPointPath);
    if (entryPoint.Length is 0)
        return 0;

    DebugLogger.WriteLine("Running Jitzu Interpreter");

    var ast = ParseProgram(entryPoint);
    var program = await ProgramBuilder.Build(ast);
    var analyser = new SemanticAnalyser(program);
    ast = analyser.AnalyseScript(ast);

    if (appArgs.Debug)
        Console.WriteLine(ExpressionFormatter.Format(ast));

    var script = new ByteCodeCompiler(program).Compile(ast.Body);
    if (appArgs.BytecodeOutputPath is not null)
        ByteCodeWriter.WriteToFile(appArgs.BytecodeOutputPath, script);

    var interpreter = new ByteCodeInterpreter(program, script, args, appArgs.Debug);
    interpreter.Evaluate();
    return 0;
}

static ScriptExpression ParseProgram(FileInfo entryPoint)
{
    DebugLogger.WriteLine($"Parsing: {entryPoint.FullName}");
    if (entryPoint.Length is 0)
    {
        DebugLogger.WriteLine("File is empty... skipping");
        return ScriptExpression.Empty;
    }

    var startTime = Stopwatch.GetTimestamp();
    try
    {
        ReadOnlySpan<char> fileContents = File.ReadAllText(entryPoint.FullName);
        if (fileContents.Length is 0)
        {
            DebugLogger.WriteLine("File is empty... skipping");
            return ScriptExpression.Empty;
        }

        StatsLogger.LogTime("File Read", Stopwatch.GetElapsedTime(startTime));

        startTime = Stopwatch.GetTimestamp();
        var lexer = new Lexer(Path.GetFullPath(entryPoint.FullName), fileContents);
        var tokens = lexer.Lex();
        StatsLogger.LogTime("Lexing", Stopwatch.GetElapsedTime(startTime));

        DebugLogger.WriteTokens(tokens);

        startTime = Stopwatch.GetTimestamp();
        var parser = new Parser(tokens);
        var program = new ScriptExpression
        {
            Body = parser.Parse(),
        };

        return program;
    }
    finally
    {
        StatsLogger.LogTime("Parsing", Stopwatch.GetElapsedTime(startTime));
    }
}
