# Jitzu.Shell Performance Issues

## Completed

| # | Issue | Fix |
|---|-------|-----|
| 1 | Git subprocess spawned every prompt | `GitStatusCache` — repo root cached by directory, branch cached by .git/HEAD mtime |
| 2 | `new string(_buffer.ToArray())` per keystroke | `CollectionsMarshal.AsSpan` for return sites; zero-alloc span for hot paths |
| 3 | `HighlightBuffer` allocates StringBuilder per keystroke | Reused field-level `_highlightSb`, writes into caller's `ArrayBufferWriter` via `GetChunks` |
| 4 | Theme dictionary lookups in highlight loop | Already `FrozenDictionary` — no issue |
| 5 | Unbuffered `Console.Write` calls during render | Synchronized output (DEC private mode 2026) |
| 6 | History `LinkedList` O(n) index walk per arrow key | Replaced with `List<string>` for O(1) access; `SetBufferFromString` memcpy |
| 7 | `FindGitRepoFolder` directory walk every prompt | Solved by #1 |
| 8 | `GetGitBranch` double file read every prompt | Solved by #1 |
| 9 | PATH enumeration on every tab press | `_pathDirectoryCache` — file names cached per PATH directory, invalidated by directory mtime |

## Remaining

### #10 — Prompt string allocations every render

**File:** `Program.cs:284-329`
**Hotness:** Per prompt (every REPL iteration)

The prompt builder creates multiple `StringBuilder` instances and a `new string(' ', padding)` allocation on every prompt render. Low impact since it's once per command, not per keystroke.

**Fix:** Reuse a field-level `StringBuilder`; replace padding string with stackalloc span.
