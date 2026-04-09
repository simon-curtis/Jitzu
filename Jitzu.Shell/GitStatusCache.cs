namespace Jitzu.Shell;

/// <summary>
/// Caches git repository root and branch name between prompt renders.
/// Repo root is cached until the working directory changes.
/// Branch is cached until .git/HEAD's modification time changes.
/// Git status is not cached — it must be fresh after every command.
/// </summary>
internal class GitStatusCache
{
    private string? _cachedDirectory;
    private DirectoryInfo? _cachedRepoRoot;
    private bool _repoRootResolved;

    private string? _cachedHeadPath;
    private DateTime _cachedHeadWriteTime;
    private string? _cachedBranch;

    /// <summary>
    /// Returns the cached git repo root, only recomputing when the working directory changes.
    /// </summary>
    public DirectoryInfo? FindGitRepoFolder(string currentDirectory)
    {
        if (_repoRootResolved && _cachedDirectory == currentDirectory)
            return _cachedRepoRoot;

        _cachedDirectory = currentDirectory;
        _cachedRepoRoot = FindGitRepoFolderCore(currentDirectory);
        _repoRootResolved = true;
        _cachedHeadPath = null;

        return _cachedRepoRoot;
    }

    /// <summary>
    /// Returns the cached branch name, only re-reading .git/HEAD when its modification time changes.
    /// </summary>
    public string? GetGitBranch(string gitRepoPath)
    {
        var gitPath = Path.Combine(gitRepoPath, ".git");

        // Handle worktrees: .git may be a file containing "gitdir: <path>"
        if (File.Exists(gitPath))
        {
            try
            {
                var gitdirLine = File.ReadAllText(gitPath).Trim();
                if (gitdirLine.StartsWith("gitdir:"))
                    gitPath = gitdirLine["gitdir:".Length..].Trim();
            }
            catch
            {
                return null;
            }
        }

        var headPath = Path.Combine(gitPath, "HEAD");
        if (!File.Exists(headPath))
            return null;

        try
        {
            var lastWrite = File.GetLastWriteTimeUtc(headPath);
            if (_cachedHeadPath == headPath && _cachedHeadWriteTime == lastWrite)
                return _cachedBranch;

            var headContent = File.ReadAllText(headPath).Trim();

            _cachedHeadPath = headPath;
            _cachedHeadWriteTime = lastWrite;

            if (headContent.StartsWith("ref: refs/heads/"))
                _cachedBranch = headContent["ref: refs/heads/".Length..];
            else if (headContent.Length >= 7)
                _cachedBranch = headContent[..7];
            else
                _cachedBranch = null;

            return _cachedBranch;
        }
        catch
        {
            return null;
        }
    }

    private static DirectoryInfo? FindGitRepoFolderCore(string path)
    {
        var dir = new DirectoryInfo(path);
        for (var depth = 0; depth < 64 && dir is not null; depth++, dir = dir.Parent)
        {
            var gitPath = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
                return dir;
        }

        return null;
    }
}
