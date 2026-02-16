using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using Jitzu.Shell.Core;
using Jitzu.Shell.UI;
using Jitzu.Shell;
using Jitzu.Core;
using Jitzu.Core.Common.Logging;
using Jitzu.Shell.Models;
using System.Reflection;

Console.OutputEncoding = Encoding.UTF8;

var options = ShellOptions.Parse(args);

// Handle elevated child entry point (launched by sudo)
if (options.SudoExec != null || options.SudoShell || options.SudoLogin)
{
    await HandleElevatedEntry(options);
    return;
}

if (options.Command is { } command)
{
    await ExecuteCommand(command);
    return;
}

if (options.ScriptPath is { } scriptPath)
{
    await ExecuteScriptFile(scriptPath);
    return;
}

await RunReplAsync(options);
return;

static async Task ExecuteCommand(string command)
{
    var theme = await ThemeConfig.LoadAsync();
    var session = await ShellSession.CreateAsync();
    var aliasManager = new AliasManager();
    await aliasManager.InitialiseAsync();
    var labelManager = new LabelManager();
    var builtins = new BuiltinCommands(session, theme, aliasManager, labelManager);
    var strategy = new ExecutionStrategy(session, builtins, aliasManager, labelManager);
    builtins.SetStrategy(strategy);

    var result = await strategy.ExecuteAsync(command);

    DisplayResult(result, theme);

    Environment.Exit(result.Error == null ? 0 : 1);
}

static async Task ExecuteScriptFile(string scriptPath)
{
    if (!File.Exists(scriptPath))
    {
        Console.WriteLine(Markup.FromString($"[red]File not found: {scriptPath}[\\]"));
        Environment.Exit(1);
    }

    var theme = await ThemeConfig.LoadAsync();
    var session = await ShellSession.CreateAsync();
    var aliasManager = new AliasManager();
    await aliasManager.InitialiseAsync();
    var labelManager = new LabelManager();
    var builtins = new BuiltinCommands(session, theme, aliasManager, labelManager);
    var strategy = new ExecutionStrategy(session, builtins, aliasManager, labelManager);
    builtins.SetStrategy(strategy);

    var scriptContent = await File.ReadAllTextAsync(scriptPath);
    var result = await strategy.ExecuteAsync(scriptContent);

    DisplayResult(result, theme);

    Environment.Exit(result.Error == null ? 0 : 1);
}

static async Task RunReplAsync(ShellOptions options)
{
    // Initialize session and components
    var theme = await ThemeConfig.LoadAsync();
    var session = await ShellSession.CreateAsync();
    var history = new HistoryManager();
    var aliasManager = new AliasManager();
    var labelManager = new LabelManager();
    var builtins = new BuiltinCommands(session, theme, aliasManager, labelManager, history);
    var strategy = new ExecutionStrategy(session, builtins, aliasManager, labelManager);
    builtins.SetStrategy(strategy);
    var completionManager = new CompletionManager(session, builtins, labelManager);
    var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    await history.InitialiseAsync();
    await aliasManager.InitialiseAsync();
    var readLine = new ReadLine(history, theme, completionManager.GetCompletions);

    // Load config file (~/.jitzu/config.jz) like .bashrc
    var configPath = Path.Combine(userProfilePath, ".jitzu", "config.jz");
    if (File.Exists(configPath))
    {
        try
        {
            var configLines = await File.ReadAllLinesAsync(configPath);
            foreach (var line in configLines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
                    await strategy.ExecuteAsync(line);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{theme["error"]}Error loading config: {ex.Message}{ThemeConfig.Reset}");
        }
    }

    // Display welcome banner
    if (options.Splash)
    {
        PrintSplash();
    }

    // State tracked between iterations for enhanced prompt
    var lastCommandSuccess = true;
    var lastCommandDuration = TimeSpan.Zero;
    var user = Environment.UserName;
    var host = Environment.MachineName;

    // Detect if running elevated (for prompt indicator)
    var isElevated = false;
    if (OperatingSystem.IsWindows())
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Main REPL loop
    while (true)
    {
        try
        {
            // Notify about completed background jobs
            var jobNotice = strategy.CheckCompletedJobs();
            if (jobNotice != null)
                Console.WriteLine(jobNotice);

            var dir = Environment.CurrentDirectory.Replace(userProfilePath, "~");

            // Trims path to root of Git repository
            // For example: D:/git/jitzu/Jitzu.Shell
            //     becomes: jitzu/Jitzu.Shell
            var gitRepoRoot = FindGitRepoFolder(Environment.CurrentDirectory);
            if (gitRepoRoot is not null)
                dir = dir.Replace(gitRepoRoot.FullName, gitRepoRoot.Name);

            var branchSuffix = "";
            if (gitRepoRoot is not null)
            {
                var branch = GetGitBranch(gitRepoRoot.FullName);
                if (branch != null)
                {
                    var status = GetGitStatus(gitRepoRoot.FullName);

                    var indicators = new StringBuilder();
                    if (status.HasDirty) indicators.Append($"{theme["git.dirty"]}*{ThemeConfig.Reset}");
                    if (status.HasStaged) indicators.Append($"{theme["git.staged"]}+{ThemeConfig.Reset}");
                    if (status.HasUntracked) indicators.Append($"{theme["git.untracked"]}?{ThemeConfig.Reset}");
                    var statusStr = indicators.Length > 0 ? $" {indicators}" : "";

                    var remoteParts = new StringBuilder();
                    if (status.Ahead > 0) remoteParts.Append($"↑{status.Ahead}");
                    if (status.Behind > 0) remoteParts.Append($"↓{status.Behind}");
                    var remoteStr = remoteParts.Length > 0 ? $" {theme["git.remote"]}{remoteParts}{ThemeConfig.Reset}" : "";

                    branchSuffix = $" {theme["git.branch"]}({branch}){ThemeConfig.Reset}{statusStr}{remoteStr}";
                }
            }

            // Build line 1: user@host dir (branch)*+? ↑1          HH:mm
            var elevatedTag = isElevated ? $" {theme["prompt.error"]}[sudo]{ThemeConfig.Reset}" : "";
            var leftPart = $"{theme["prompt.user"]}{user}@{host}{ThemeConfig.Reset} {theme["prompt.directory"]}{dir}{ThemeConfig.Reset}{branchSuffix}{elevatedTag}";
            var visibleLeft = Markup.Remove(leftPart).Length;
            var timeStr = DateTime.Now.ToString("HH:mm");
            var bufferWidth = Console.BufferWidth;
            var padding = Math.Max(1, bufferWidth - visibleLeft - timeStr.Length);
            var line1 = $"{leftPart}{new string(' ', padding)}{theme["prompt.time"]}{timeStr}{ThemeConfig.Reset}";

            // Build line 2 (optional): [N] took Xs
            var line2Parts = new StringBuilder();
            var activeJobs = strategy.Jobs.Count(j => !j.Process.HasExited);
            if (activeJobs > 0)
                line2Parts.Append($"{theme["prompt.jobs"]}[{activeJobs}]{ThemeConfig.Reset} ");

            if (lastCommandDuration.TotalSeconds >= 2)
            {
                var durationStr = lastCommandDuration.TotalMinutes >= 1
                    ? $"{(int)lastCommandDuration.TotalMinutes}m {lastCommandDuration.Seconds}s"
                    : $"{(int)lastCommandDuration.TotalSeconds}s";
                line2Parts.Append($"{theme["prompt.duration"]}took {durationStr}{ThemeConfig.Reset}");
            }

            var line2 = line2Parts.Length > 0 ? $"{line2Parts}\n" : "";

            // Build line 3: arrow colored by last command success
            var arrowColor = lastCommandSuccess ? theme["prompt.arrow"] : theme["prompt.error"];
            var promptChar = isElevated ? "#" : "❯";

            var prompt = $"{line1}\n{line2}{arrowColor}{ThemeConfig.Bold}{promptChar}{ThemeConfig.Reset} ";
            var line = readLine.Read(prompt);

            if (line is "exit")
                return;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            await history.WriteAsync(line);

            var sw = Stopwatch.StartNew();
            var result = await strategy.ExecuteAsync(line);
            sw.Stop();

            lastCommandDuration = sw.Elapsed;
            lastCommandSuccess = result.Error == null;

            DisplayResult(result, theme);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{theme["error"]}Unexpected error: {ex.Message}{ThemeConfig.Reset}");
        }
    }
}

static async Task HandleElevatedEntry(ShellOptions options)
{
    var parentPid = options.ParentPid;

    // Attach to parent's console for seamless terminal experience
    if (parentPid > 0)
    {
        if (!SudoCommand.AttachToParentConsole(parentPid))
        {
            Console.Error.WriteLine("sudo: failed to attach to parent console");
            Environment.Exit(1);
            return;
        }
    }

    if (options.SudoExec is { } command)
    {
        // Mode 1: Run single command elevated, then exit
        var theme = await ThemeConfig.LoadAsync();
        var session = await ShellSession.CreateAsync();
        var aliasManager = new AliasManager();
        await aliasManager.InitialiseAsync();
        var labelManager = new LabelManager();
        var builtins = new BuiltinCommands(session, theme, aliasManager, labelManager);
        var strategy = new ExecutionStrategy(session, builtins, aliasManager, labelManager);
        builtins.SetStrategy(strategy);

        var result = await strategy.ExecuteAsync(command);
        DisplayResult(result, theme);

        Environment.Exit(result.Error == null ? 0 : 1);
    }
    else
    {
        // Mode 2: Shell takeover — kill parent and run REPL
        if (parentPid > 0)
        {
            try
            {
                var parent = Process.GetProcessById(parentPid);
                parent.Kill();
            }
            catch
            {
                // Parent may have already exited
            }
        }

        if (options.SudoLogin)
        {
            // Login shell: reset to user profile directory
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            Environment.CurrentDirectory = userProfile;
        }

        await RunReplAsync(options);
    }
}

static void PrintSplash()
{
    /*
    jitzu v0.3.0 — mindful shell environment

    • platform   darwin-arm64
    • runtime    node 20.11.0
    • config     ~/.jitzu/config.toml
    • session    interactive
    */

    var sb = new StringBuilder();
    sb.AppendLine($"jzsh v{Assembly.GetExecutingAssembly().GetName().Version}");
    sb.AppendLine();
    sb.AppendLine($"• runtime    : {Environment.Version}");
    sb.AppendLine("• config     : ~/.jitzu/config.jz");
    sb.AppendLine($"• platform   : {Environment.OSVersion.Platform}");
    sb.AppendLine();
    sb.AppendLine("Type `help` to get started.");

    Console.WriteLine(sb.ToString());
}


static void DisplayResult(ShellResult result, ThemeConfig theme)
{
    if (result.Error != null)
    {
        // Display error using ExceptionPrinter if it's a JitzuException
        if (result.Error is JitzuException jitzuEx)
        {
            ExceptionPrinter.Print(jitzuEx);
        }
        else
        {
            Console.WriteLine($"{theme["error"]}{result.Error.Message}{ThemeConfig.Reset}");
        }
    }
    else if (!string.IsNullOrWhiteSpace(result.Output))
    {
        // Display output
        Console.WriteLine(result.Output);
    }
}

/// <summary>
/// Detects if the current directory is inside a Git repository by climbing the parent directories
/// until it finds a .git directory or root.
/// </summary>
/// <returns>The path to the root of the Git repository or null if not found</returns>
static DirectoryInfo? FindGitRepoFolder(string path)
{
    var directoryInfo = new DirectoryInfo(path);
    var gitPath = Path.Combine(directoryInfo.FullName, ".git");

    if (Directory.Exists(gitPath) || File.Exists(gitPath))
        return directoryInfo;

    var parent = directoryInfo.Parent;
    return parent != null ? FindGitRepoFolder(parent.FullName) : null;
}

/// <summary>
/// Gets git working tree status by running `git status --porcelain --branch`.
/// Returns indicators for dirty, staged, untracked files and ahead/behind counts.
/// </summary>
static (bool HasStaged, bool HasDirty, bool HasUntracked, int Ahead, int Behind) GetGitStatus(string gitRepoPath)
{
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = gitRepoPath,
        };
        startInfo.ArgumentList.Add("status");
        startInfo.ArgumentList.Add("--porcelain=v1");
        startInfo.ArgumentList.Add("--branch");

        using var process = Process.Start(startInfo);
        if (process == null)
            return default;

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            return default;

        var hasStaged = false;
        var hasDirty = false;
        var hasUntracked = false;
        var ahead = 0;
        var behind = 0;

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("##"))
            {
                // Parse: ## branch...origin/branch [ahead N, behind M]
                var bracketStart = line.IndexOf('[');
                if (bracketStart >= 0)
                {
                    var bracketEnd = line.IndexOf(']', bracketStart);
                    if (bracketEnd >= 0)
                    {
                        var info = line[(bracketStart + 1)..bracketEnd];
                        foreach (var part in info.Split(',', StringSplitOptions.TrimEntries))
                        {
                            if (part.StartsWith("ahead ") && int.TryParse(part.AsSpan()[6..], out var a))
                                ahead = a;
                            else if (part.StartsWith("behind ") && int.TryParse(part.AsSpan()[7..], out var b))
                                behind = b;
                        }
                    }
                }
                continue;
            }

            if (line.Length < 2) continue;

            if (line.StartsWith("??"))
            {
                hasUntracked = true;
                continue;
            }

            // First column: staging area status
            if (line[0] is not ' ' and not '?')
                hasStaged = true;

            // Second column: working tree status
            if (line[1] is not ' ' and not '?')
                hasDirty = true;
        }

        return (hasStaged, hasDirty, hasUntracked, ahead, behind);
    }
    catch
    {
        return default;
    }
}

/// <summary>
/// Reads the current git branch name by parsing .git/HEAD directly.
/// Returns branch name, short SHA for detached HEAD, or null on error.
/// </summary>
static string? GetGitBranch(string gitRepoPath)
{
    try
    {
        var gitPath = Path.Combine(gitRepoPath, ".git");

        // Handle worktrees: .git may be a file containing "gitdir: <path>"
        if (File.Exists(gitPath))
        {
            var gitdirLine = File.ReadAllText(gitPath).Trim();
            if (gitdirLine.StartsWith("gitdir:"))
                gitPath = gitdirLine["gitdir:".Length..].Trim();
        }

        var headPath = Path.Combine(gitPath, "HEAD");
        if (!File.Exists(headPath))
            return null;

        var headContent = File.ReadAllText(headPath).Trim();

        // Symbolic ref: "ref: refs/heads/branch-name"
        if (headContent.StartsWith("ref: refs/heads/"))
            return headContent["ref: refs/heads/".Length..];

        // Detached HEAD: return short SHA
        if (headContent.Length >= 7)
            return headContent[..7];

        return null;
    }
    catch
    {
        return null;
    }
}
