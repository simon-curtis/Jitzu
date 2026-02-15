using Jitzu.Core;
using Jitzu.Core.Language;
using Jitzu.Core.Runtime;
using Jitzu.Core.Runtime.Compilation;
using Jitzu.Shell.Core.Completions;

namespace Jitzu.Shell.Core;

/// <summary>
/// Maintains persistent state across REPL iterations.
/// This is the KEY to stateful execution - we DON'T recreate RuntimeProgram each time.
/// </summary>
public class ShellSession
{
    // Persistent compilation state
    private ProgramStack _stack = null!;

    // NEW: persistent runtime assembly universe
    private readonly ReplLoadContext _loadContext = new();
    private readonly PackageResolver _resolver = new();

    // Expose program for introspection (used by builtin commands)
    public RuntimeProgram Program { get; private set; } = null!;

    public static async Task<ShellSession> CreateAsync()
    {
        var session = new ShellSession();
        await session.Initialize();
        return session;
    }

    private async Task Initialize()
    {
        // Initialize with built-in types and functions
        Program = await InitializeBaseProgram();

        // Create persistent stack and initialize with global functions and types
        _stack = new ProgramStack();
        _stack.SetGlobal(0, Value.FromRef(Array.Empty<string>())); // args slot

        InitializeGlobalStack();
    }

    private static async Task<RuntimeProgram> InitializeBaseProgram()
    {
        // Use ProgramBuilder.Build() with empty AST
        // This gives us all built-in types and global functions
        var emptyScript = ScriptExpression.Empty;
        return await ProgramBuilder.Build(emptyScript);
    }

    private void InitializeGlobalStack()
    {
        // Initialize global slots with types and functions
        foreach (var (name, index) in Program.GlobalSlotMap)
        {
            if (Program.GlobalFunctions.TryGetValue(name, out var function))
                _stack.SetGlobal(index, Value.FromRef(function));
            else if (Program.Types.TryGetValue(name, out var type) 
                     || Program.SimpleTypeCache.TryGetValue(name, out type))
                _stack.SetGlobal(index, Value.FromRef(type));
        }
    }

    /// <summary>
    /// Execute a single line or block incrementally.
    /// Returns: (success: bool, result: object?, error: Exception?)
    /// </summary>
    public async Task<ExecutionResult> ExecuteAsync(string input)
    {
        try
        {
            // Parse the new input
            var newAst = Parser.Parse("<repl>", input);
            var scriptExpression = new ScriptExpression
            {
                Body = newAst,
                Location = SourceSpan.Empty
            };

            Program = await ProgramBuilder.PatchProgram(Program, scriptExpression);

            // Run semantic analysis (type resolution, function registration)
            var analyser = new SemanticAnalyser(Program);
            scriptExpression = analyser.AnalyseScript(scriptExpression);

            // Update global stack with new program state (types, functions, updated SlotMap)
            InitializeGlobalStack();

            // Extract and compile only the new expressions
            var script = new ByteCodeCompiler(Program).Compile(scriptExpression.Body);

            // Execute using persistent stack to maintain global variables
            var interpreter = new ByteCodeInterpreter(Program, script, _stack, false);
            var result = interpreter.Evaluate();

            return new ExecutionResult(true, result, null);
        }
        catch (Exception ex)
        {
            return new ExecutionResult(false, null, ex);
        }
    }

    public async Task ResetAsync() => await Initialize();

    public List<Completion> GetCompletionSuggestions(string partial)
    {
        // Return variable names, function names, type names
        var suggestions = new List<Completion>();

        // Global functions
        suggestions.AddRange(
            Program.GlobalFunctions
                .Where(f => f.Key.StartsWith(partial))
                .Select(f => new RuntimeFunctionCompletion(f.Key)));

        // Types (simple names)
        suggestions.AddRange(
            Program.SimpleTypeCache
                .Where(f => f.Key.StartsWith(partial))
                .Select(f => new RuntimeFunctionCompletion(f.Key)));

        // Keywords
        var keywords = new[] { "let", "fun", "type", "if", "else", "match", "return", "true", "false", "pub" };
        suggestions.AddRange(
            keywords
                .Where(k => k.StartsWith(partial))
                .Select(k => new KeywordCompletion(k)));

        return suggestions;
    }

    
}

public record ExecutionResult(bool Success, object? Result, Exception? Error);