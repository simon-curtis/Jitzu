# Upgrade & Distribution Strategy

**Status:** Ready for review
**Phase:** REFINE

## Problem Statement

**Who:** Jitzu users on Windows, macOS, and Linux
**What:** No automated upgrade path — users must manually download ZIPs from GitHub Releases. Two separate binaries (`jz` + `jzsh`) complicate distribution and packaging.
**Why it matters:** Friction kills adoption. One-command install/upgrade is table stakes for CLI tools. A single binary simplifies every distribution channel.
**Evidence:** Version was stuck at 0.1.0 even after multiple releases (just fixed via Directory.Build.props)

## Current State

- **Distribution:** GitHub Releases only (4 platform ZIPs, each containing `jz` + `jzsh`)
- **Install process:** Manual download → extract → add to PATH
- **Upgrade process:** Same as install (fully manual)
- **No package manager presence** — no Scoop, WinGet, Homebrew, or apt manifests exist
- **Two separate binaries** with separate projects (`Jitzu.Interpreter` → `jz` 86 MB, `Jitzu.Shell` → `jzsh` 76 MB)
- **Combined ZIP download:** ~162 MB unpacked per platform

## Proposed Solution

Merge into a single `jz` binary (~87-90 MB, halving download size), then distribute via four package managers + a built-in self-update command. Ship as **v0.2.0** to signal the breaking change. Clean break — no `jzsh` backward compat.

---

## D0 — Merge Binaries into Single `jz` (M)

### Design

Single binary named `jz`. Mode determined by arguments:

| Invocation | Behavior |
|------------|----------|
| `jz` | Shell mode (interactive REPL) |
| `jz script.jz` | Interpreter mode (full-file compilation + execute) |
| `jz -c "command"` | Single command execution (shell-style, through ExecutionStrategy) |
| `jz run script.jz` | Explicit interpreter mode (alternate syntax) |
| `jz upgrade` | Self-update |
| `jz --version` | Print version |
| `jz --debug script.jz` | Debug interpreter mode |

### File vs subcommand dispatch

The first positional argument is resolved as follows:

```
1. Is it a known subcommand ("run", "upgrade")? → dispatch to subcommand
2. Does File.Exists(arg) return true?            → interpreter mode (full-file compilation)
3. Otherwise                                     → error: "File not found: {arg}"
```

This avoids ambiguity — there's no OS-command fallback from the top-level `jz` dispatch. OS commands are only available inside the shell (interactive REPL or `-c`).

### Two execution paths (preserved, not merged)

| Mode | Entry | Compilation | Execution |
|------|-------|-------------|-----------|
| **Interpreter** (`jz file.jz`) | Full-file read | `ProgramBuilder.Build()` → `SemanticAnalyser` → `AstTransformer` → `ByteCodeCompiler` | Single `ByteCodeInterpreter.Evaluate()` run |
| **Shell REPL** (`jz` with no args) | Line-by-line input | `ProgramBuilder.PatchProgram()` (incremental) → per-line compile | Persistent `ProgramStack` across evaluations |
| **Shell single command** (`jz -c "..."`) | String input | Routes through `ExecutionStrategy` | OS fallback, alias expansion, pipes, chaining |

The interpreter path uses `ProgramBuilder.Build()` (full compilation) — NOT the Shell's `ExecutionStrategy` (which does line-by-line with OS fallback). These are fundamentally different and both are preserved.

### Architecture

**Current state:**
- `Jitzu.Shell` → depends on `Jitzu.Core` only (does NOT reference `Jitzu.Interpreter`)
- `Jitzu.Interpreter` → depends on `Jitzu.Core` only
- Both projects independently invoke the same Core compilation pipeline

**Merge strategy:** Absorb `Jitzu.Interpreter`'s unique functionality into `Jitzu.Shell`, since Shell is the superset:

| Interpreter-only feature | What moves |
|--------------------------|-----------|
| `--debug` flag + `DebugLogger` | Move to merged CLI args |
| `--bytecode-output` + `ByteCodeWriter` (AsmResolver) | Move to merged CLI args |
| `--telemetry` + `StatsLogger` | Move to merged CLI args |
| `--install-path` | Move to merged CLI args |
| Full-file compilation orchestration (`Program.cs:72-85`) | Move to a `ScriptRunner` class in the merged project |

**What stays the same:**
- `Jitzu.Core` — unchanged
- All 70 Shell builtin commands — unchanged
- Custom readline, completions, themes — unchanged
- `ShellSession` incremental execution — unchanged

**Merged CLI argument model:**
```
jz [OPTIONS] [file] [-- script-args...]
jz <COMMAND>

Commands:
  run <file>     Run a script file (explicit)
  upgrade        Update jz to the latest version

Options:
  -d, --debug          Enable debug output
  -b <path>            Write bytecode to file
  -t, --telemetry      Show execution telemetry
  -c <command>         Execute a single command
  --no-splash          Suppress shell splash banner
  --install-path       Print install directory
  --version            Print version
```

**Entry point logic:**
```
if (args has --install-path)                → print install dir, exit
if (args has file or "run" subcommand)      → full-file interpreter mode (ProgramBuilder.Build path)
else if (args has -c)                       → single command execution (ExecutionStrategy path)
else if (args has "upgrade" subcommand)     → self-update
else                                        → interactive shell (REPL)
```

### Binary size

Measured on win-x64 self-contained single-file publish:

| Binary | Current size |
|--------|-------------|
| `jz.exe` (Interpreter) | 86.4 MB |
| `jzsh.exe` (Shell) | 75.6 MB |
| **Combined download (both)** | **162 MB** unpacked |
| **Merged binary (estimated)** | **~87-90 MB** |

The delta is ~11 MB from Interpreter-only deps (`Microsoft.Data.SqlClient`, `AsmResolver.DotNet`, `Microsoft.Extensions.Http`). The merged binary is essentially the Interpreter's size since shared .NET runtime and `Jitzu.Core` deduplicate. **Net effect: download size halves.**

### Project changes

1. **Rename `Jitzu.Shell` → `Jitzu` (or keep as-is internally)**, change `AssemblyName` to `jz`
2. **Move** interpreter-only files from `Jitzu.Interpreter` into the merged project:
   - `Infrastructure/DebugLogger.cs`
   - `Infrastructure/ExpressionFormatter.cs`
   - `Infrastructure/StatsLogger.cs`
   - `Core/ByteCodeWriter.cs` (if keeping bytecode output feature)
   - Full-file orchestration logic from `Jitzu.Interpreter/Program.cs:72-85` → new `ScriptRunner` class
3. **Merge NuGet dependencies** — add `AsmResolver.DotNet`, `CliWrap`, `ConsoleTables`, `Microsoft.Data.SqlClient`, `Microsoft.Extensions.Http` to the merged csproj
4. **Unify CLI arg model** — combine `ShellOptions` + `JitzuCli` into one `Clap.Net` model
5. **Update `Program.cs`** — single dispatch that routes to shell or interpreter based on args
6. **Remove `Jitzu.Interpreter` project** from solution
7. **Update `Jitzu.Tests`** — retarget any Interpreter project references to the merged project
8. **Update `Jitzu.Benchmarking`** — retarget references if applicable
9. **Update CI** — publish one binary instead of two, update ZIP contents
10. **Update docs site** — installation page, remove references to `jzsh`

### Breaking changes

- `jzsh` binary removed entirely (clean break, no shim)
- Users who had `jzsh` in scripts/aliases need to switch to `jz`
- Shell-specific flags (`--sudo-exec`, `--sudo-shell`, etc.) remain on `jz` since it IS the shell now

---

## D1 — Scoop Bucket (S)

*Depends on: D0*

Create GitHub repo `simon-curtis/scoop-jitzu` with auto-generated manifest.

**Manifest** (`jitzu.json`):
```json
{
  "version": "0.2.0",
  "architecture": {
    "64bit": {
      "url": "https://github.com/simon-curtis/Jitzu/releases/download/v0.2.0/jitzu-0.2.0-win-x64.zip",
      "hash": "<sha256>"
    }
  },
  "bin": ["jz.exe"],
  "checkver": { "github": "https://github.com/simon-curtis/Jitzu" },
  "autoupdate": {
    "architecture": {
      "64bit": {
        "url": "https://github.com/simon-curtis/Jitzu/releases/download/v$version/jitzu-$version-win-x64.zip"
      }
    }
  }
}
```

**CI automation:** Post-release job proactively computes SHA256 hash, updates manifest, pushes to bucket repo. This ensures immediate availability rather than waiting for Scoop's own `autoupdate` polling.

**Infra prerequisite:** CI needs a PAT or deploy key with push access to the `scoop-jitzu` repo (the default `GITHUB_TOKEN` is scoped to the current repo only). Store as a repository secret.

**User experience:**
```powershell
scoop bucket add jitzu https://github.com/simon-curtis/scoop-jitzu
scoop install jitzu
# Later...
scoop update jitzu
```

## D2 — Homebrew Tap (S-M)

*Depends on: D0*

Create GitHub repo `simon-curtis/homebrew-jitzu` (follows Homebrew naming convention: `brew tap simon-curtis/jitzu` maps to repo `simon-curtis/homebrew-jitzu`).

**Formula** (`Formula/jitzu.rb`):
```ruby
class Jitzu < Formula
  desc "A fast and flexible script execution engine"
  homepage "https://github.com/simon-curtis/Jitzu"
  version "0.2.0"

  on_macos do
    if Hardware::CPU.arm?
      url "https://github.com/simon-curtis/Jitzu/releases/download/v0.2.0/jitzu-0.2.0-osx-arm64.zip"
      sha256 "<hash>"
    else
      url "https://github.com/simon-curtis/Jitzu/releases/download/v0.2.0/jitzu-0.2.0-osx-x64.zip"
      sha256 "<hash>"
    end
  end

  on_linux do
    url "https://github.com/simon-curtis/Jitzu/releases/download/v0.2.0/jitzu-0.2.0-linux-x64.zip"
    sha256 "<hash>"
  end

  def install
    bin.install "jz"
  end
end
```

**CI automation:** Post-release job computes SHA256 hashes, updates formula, pushes to tap repo.

**Infra prerequisite:** Same as D1 — PAT or deploy key for the `homebrew-jitzu` repo.

**User experience:**
```bash
brew tap simon-curtis/jitzu
brew install jitzu
# Later...
brew upgrade jitzu
```

## D3 — Self-Update Command: `jz upgrade` (M)

*Depends on: D0*

### Flow

```
jz upgrade
  1. GET https://api.github.com/repos/simon-curtis/Jitzu/releases/latest
  2. Parse tag_name (e.g. "v0.2.1") → strip "v" → semver string "0.2.1"
  3. Compare to Assembly.GetExecutingAssembly().GetName().Version
     Note: Assembly version is 4-part (0.2.1.0), tag is 3-part (0.2.1).
     Compare only major.minor.build.
  4. If up to date → "Already on latest (v0.2.0)"
  5. If newer:
     a. Detect platform (see platform detection below)
     b. Find matching asset URL (jitzu-{version}-{rid}.zip)
     c. Download ZIP to temp directory
     d. Extract jz binary from ZIP
     e. Replace current binary (see platform strategy below)
     f. Print "Updated to v0.2.1! Restart to use new version."
```

### Platform detection

`RuntimeInformation.RuntimeIdentifier` may be empty for non-RID-specific builds. Use a fallback chain:

```csharp
string rid = RuntimeInformation.RuntimeIdentifier;
if (string.IsNullOrEmpty(rid))
{
    string arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) rid = $"win-{arch}";
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) rid = $"osx-{arch}";
    else rid = $"linux-{arch}";
}
```

### Binary replacement strategy

**Windows** (cannot overwrite running .exe, but CAN rename it):
```
1. Rename  jz.exe  →  jz.exe.old     (works while running)
2. Write   new jz.exe from temp to same path
3. Print   "Updated to vX.Y.Z! Restart to use new version."
4. On next startup: detect and delete jz.exe.old
```

**Unix** (write-to-temp-then-rename for atomicity):
```
1. Write   new jz to temp file in same directory (jz.tmp)
2. chmod   +x jz.tmp
3. Rename  jz.tmp → jz   (atomic on same filesystem, overwrites old)
4. Print   "Updated to vX.Y.Z! Restart to use new version."
```

This avoids the gap where the binary is deleted but not yet written (disk full, permission error after unlink). Rename-over is atomic on Unix when source and target are on the same filesystem.

### Package manager detection

Before self-updating, check if installed via a package manager:
- **Scoop:** binary path contains `scoop/apps/jitzu/` → warn: "Installed via Scoop. Use `scoop update jitzu` instead."
- **Homebrew:** binary path starts with `/opt/homebrew/`, `/usr/local/Cellar/`, or `/home/linuxbrew/` → warn: "Installed via Homebrew. Use `brew upgrade jitzu` instead."

User can override with `jz upgrade --force`.

### Error handling

- No write permission → "Cannot update: permission denied. Try running with elevated privileges."
- Network failure → "Cannot reach GitHub. Check your connection."
- Download interrupted → temp file cleaned up, original binary untouched (rename hasn't happened yet)
- Rename fails (Windows file lock edge case) → "Cannot update: file is locked. Close all jz instances and try again."

## D4 — WinGet Manifest (M)

*Depends on: D0*

Submit manifest to `microsoft/winget-pkgs`.

**Manifest** (`manifests/s/SimonCurtis/Jitzu/0.2.0/`):
```yaml
PackageIdentifier: SimonCurtis.Jitzu
PackageVersion: 0.2.0
InstallerType: zip
Installers:
  - Architecture: x64
    InstallerUrl: https://github.com/simon-curtis/Jitzu/releases/download/v0.2.0/jitzu-0.2.0-win-x64.zip
    InstallerSha256: <hash>
DefaultLocale: en-US
PackageName: Jitzu
Publisher: Simon Curtis
ShortDescription: A fast and flexible script execution engine
```

**CI automation:** Use `wingetcreate` to auto-submit PR to `microsoft/winget-pkgs` on each release. Microsoft review ~24-48h per submission.

## D5 — Apt Repository (L) — DEFERRED

Deferred until there's meaningful Linux adoption demand. Homebrew covers Linux already. If needed later:
- `.deb` packaging in CI
- GitHub Pages-hosted apt repo via `reprepro`
- GPG key management via GitHub secrets

---

## Recommended Order

| # | Deliverable | Effort | Rationale |
|---|-------------|--------|-----------|
| 0 | **Merge binaries** | M | Prerequisite — simplifies everything downstream |
| 1 | **Scoop** | S | Fastest win, covers Windows |
| 2 | **Homebrew** | S-M | Covers macOS + Linux |
| 3 | **Self-update** | M | Fallback for manual installs |
| 4 | **WinGet** | M | Nice to have, slower review cycle |
| 5 | ~~Apt~~ | ~~L~~ | Deferred |

## CI Automation Strategy

Update `build-release.yml`:

**Build job changes:**
- Remove Shell publish step
- Single `dotnet publish` of the merged project → `jz` / `jz.exe`
- ZIP contains one file instead of two

**New post-release job:**
```yaml
update-package-managers:
  needs: release
  steps:
    - Download release assets
    - Compute SHA256 for each platform ZIP
    - Update Scoop bucket repo (git push to simon-curtis/scoop-jitzu)
    - Update Homebrew tap formula (git push to simon-curtis/homebrew-jitzu)
    - Submit WinGet manifest via wingetcreate (PR to microsoft/winget-pkgs)
```

**Infra prerequisites:**
- GitHub PAT (or fine-grained token) with push access to `scoop-jitzu` and `homebrew-jitzu` repos, stored as repository secret (e.g., `PKG_DEPLOY_TOKEN`)
- `wingetcreate` needs a GitHub token for PR submission

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Binary merge breaks shell or interpreter behavior | Medium | High | Thorough testing of both modes; existing test suite + manual smoke tests |
| Self-update writes partial binary (crash/disk full) | Low | High | Write to temp first, then atomic rename; `.old` kept as rollback on Windows |
| Self-update conflicts with package manager state | Medium | Medium | Auto-detect install method via path heuristics, warn user |
| WinGet review delays | Medium | Low | Scoop is primary Windows channel |
| GitHub API rate limits on `jz upgrade` | Low | Low | 60 req/hr unauthenticated — fine for CLI |
| PAT/deploy key rotation burden | Low | Low | Use fine-grained tokens scoped to specific repos |

## Trade-offs

| Chose | Over | Because |
|-------|------|---------|
| Single binary | Separate `jz` + `jzsh` | Simpler distribution, install, upgrade, PATH management. Download halves. |
| Shell as default mode | Interpreter as default | Shell is the "home base"; scripts are explicit via file arg |
| Full-file compilation for `jz file.jz` | Shell's ExecutionStrategy | Interpreter path is faster, stricter, no OS fallback. Correct semantics for script files. |
| Clean break (no jzsh shim) | Backward compat symlink | Project is pre-1.0, few users, not worth the maintenance |
| Atomic rename on Unix | Delete-then-write | No window where binary is absent; survives disk-full mid-write |
| Rename-then-write on Windows | Helper process / scheduled task | Simpler, no external dependencies, proven pattern (rustup, gh) |
| Scoop + Homebrew first | WinGet + Apt first | Lower barrier, faster automation, wider reach |
| Defer Apt | Build it now | High maintenance (GPG, hosting), Homebrew covers Linux already |
| Separate repos per package manager | Mono-repo | Follows conventions (`scoop-*`, `homebrew-*`), simpler CI permissions |

## Non-Goals

- Auto-update check on shell startup (explicitly excluded per user preference)
- Linux ARM builds (no `linux-arm64` in current CI matrix)
- Chocolatey, Snap, Flatpak, or other package managers
- `jzsh` backward compatibility shim

## Acceptance Criteria

- [ ] Single `jz` binary launches shell with no args
- [ ] `jz file.jz` runs full-file compilation (ProgramBuilder.Build path), not ExecutionStrategy
- [ ] `jz -c "command"` runs through ExecutionStrategy (shell-style)
- [ ] `jz --version` prints correct version from assembly metadata
- [ ] `jz upgrade` detects current version, checks GitHub, downloads and replaces binary
- [ ] `jz upgrade` on Windows: rename-then-write, cleanup `.old` on next launch
- [ ] `jz upgrade` on Unix: write-to-temp-then-rename (atomic)
- [ ] `jz upgrade` warns when installed via Scoop/Homebrew
- [ ] `scoop install jitzu` works from `simon-curtis/scoop-jitzu` bucket
- [ ] `brew install simon-curtis/jitzu/jitzu` works from `simon-curtis/homebrew-jitzu` tap
- [ ] CI publishes single binary per platform, creates release, updates package manager repos
- [ ] All existing tests pass against merged binary
- [ ] Docs site installation page updated

## Migration Notes (for v0.2.0 release)

Release notes should include:
- **Breaking:** `jzsh` removed. Use `jz` for both shell and interpreter.
- **New:** `jz` with no arguments launches the shell (previously required `jzsh`)
- **New:** `jz upgrade` for self-updating
- **New:** Available via `scoop install jitzu` (Windows) and `brew install simon-curtis/jitzu/jitzu` (macOS/Linux)
