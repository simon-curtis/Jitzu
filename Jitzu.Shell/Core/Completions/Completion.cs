namespace Jitzu.Shell.Core.Completions;

public abstract record Completion(string Value, int Priority);

public record DirectoryCompletion(string Value) : Completion(Value, 0);

public record FileCompletion(string Value) : Completion(Value, 1);

public record KeywordCompletion(string Value) : Completion(Value, 2);

public record RuntimeFunctionCompletion(string Value) : Completion(Value, 3);

public record ExecutableCompletion(string Value) : Completion(Value, 4);