import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/commands")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Built-in Commands</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell includes 60+ built-in commands implemented natively with colored output, cross-platform support, and tight integration with the rest of the shell.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>File System</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>ls [dir]</code></td><td className="py-2">List files with colored output, attributes, and sizes</td></tr>
              <tr><td className="py-2 pr-4"><code>cd [dir]</code></td><td className="py-2">Change directory (<code>cd -</code> for previous, <code>cd</code> for home)</td></tr>
              <tr><td className="py-2 pr-4"><code>pwd</code></td><td className="py-2">Print working directory</td></tr>
              <tr><td className="py-2 pr-4"><code>mkdir [-cd] dir</code></td><td className="py-2">Create directory (<code>-cd</code> to enter it immediately)</td></tr>
              <tr><td className="py-2 pr-4"><code>touch [-d/-t] file</code></td><td className="py-2">Create file or update timestamp</td></tr>
              <tr><td className="py-2 pr-4"><code>rm [-r] path</code></td><td className="py-2">Remove file or directory</td></tr>
              <tr><td className="py-2 pr-4"><code>mv src dst</code></td><td className="py-2">Move or rename file/directory</td></tr>
              <tr><td className="py-2 pr-4"><code>cp [-r] src dst</code></td><td className="py-2">Copy file or directory</td></tr>
              <tr><td className="py-2 pr-4"><code>ln [-s] target link</code></td><td className="py-2">Create hard or symbolic link</td></tr>
              <tr><td className="py-2 pr-4"><code>stat file</code></td><td className="py-2">Display file metadata</td></tr>
              <tr><td className="py-2 pr-4"><code>chmod +/-r file</code></td><td className="py-2">Toggle file attributes (r/h/s)</td></tr>
              <tr><td className="py-2 pr-4"><code>find path [opts]</code></td><td className="py-2">Recursive file search</td></tr>
              <tr><td className="py-2 pr-4"><code>du [-sh] [dir]</code></td><td className="py-2">Disk usage of files/directories</td></tr>
              <tr><td className="py-2 pr-4"><code>df</code></td><td className="py-2">Show disk space of mounted drives</td></tr>
              <tr><td className="py-2 pr-4"><code>mktemp [-d]</code></td><td className="py-2">Create temporary file or directory</td></tr>
              <tr><td className="py-2 pr-4"><code>basename path [suffix]</code></td><td className="py-2">Strip directory and optional suffix</td></tr>
              <tr><td className="py-2 pr-4"><code>dirname path</code></td><td className="py-2">Strip last component from path</td></tr>
            </tbody>
          </table>
        </div>

        <Heading3>ls</Heading3>
        <p>
          On Windows, <code className="text-sm bg-muted px-2 py-1 rounded">ls</code> is a builtin that shows file attributes, sizes, timestamps, and color-coded names by file type. On other platforms it falls through to the system <code className="text-sm bg-muted px-2 py-1 rounded">ls</code>.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ ls
d-----       -  Jan 15 09:44  src/
d-----       -  Jan 10 11:20  tests/
-a-r--   2.5K  Jan 15 14:23  Program.cs
-a-r--   1.8K  Jan 12 10:05  README.md`}
        />

        <Heading3>find</Heading3>
        <p>Recursive file search with filtering by name pattern, type, and extension:</p>
        <CodeBlock
          language="bash"
          code={`❯ find src -ext cs
src/Parser.cs
src/Lexer.cs
src/Interpreter.cs

❯ find . -name "*.json" -type f
./config.json
./package.json`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Text Processing</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>cat file</code></td><td className="py-2">Display file with line numbers</td></tr>
              <tr><td className="py-2 pr-4"><code>head [-n N] file</code></td><td className="py-2">Show first N lines (default 10)</td></tr>
              <tr><td className="py-2 pr-4"><code>tail [-n N] file</code></td><td className="py-2">Show last N lines (default 10)</td></tr>
              <tr><td className="py-2 pr-4"><code>grep [flags] pattern [files]</code></td><td className="py-2">Search files for pattern</td></tr>
              <tr><td className="py-2 pr-4"><code>sort [-r/-n/-u] file</code></td><td className="py-2">Sort lines</td></tr>
              <tr><td className="py-2 pr-4"><code>uniq [-c/-d] file</code></td><td className="py-2">Remove consecutive duplicate lines</td></tr>
              <tr><td className="py-2 pr-4"><code>wc [-l/-w/-c] file</code></td><td className="py-2">Count lines, words, or characters</td></tr>
              <tr><td className="py-2 pr-4"><code>diff file1 file2</code></td><td className="py-2">Compare two files</td></tr>
              <tr><td className="py-2 pr-4"><code>tr [-d] s1 s2 file</code></td><td className="py-2">Translate or delete characters</td></tr>
              <tr><td className="py-2 pr-4"><code>cut -d/-f/-c file</code></td><td className="py-2">Extract fields or characters</td></tr>
              <tr><td className="py-2 pr-4"><code>rev file</code></td><td className="py-2">Reverse characters in each line</td></tr>
              <tr><td className="py-2 pr-4"><code>tac file</code></td><td className="py-2">Print file with lines in reverse order</td></tr>
              <tr><td className="py-2 pr-4"><code>paste [-d] file file</code></td><td className="py-2">Merge lines from multiple files</td></tr>
              <tr><td className="py-2 pr-4"><code>more/less</code></td><td className="py-2">Paginated output viewer</td></tr>
            </tbody>
          </table>
        </div>

        <Heading3>grep</Heading3>
        <p>
          Search for patterns in files with highlighted matches. Supports recursive search, case-insensitive matching, line numbers, and match counting.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ grep -rn "TODO" src/
src/Parser.cs:42:    // TODO: Handle forward references
src/Interpreter.cs:156:    // TODO: Optimize stack layout

❯ grep -ic "error" logs/
logs/app.log:15
logs/server.log:3`}
        />
        <p>Flags: <code className="text-sm bg-muted px-2 py-1 rounded">-i</code> case insensitive, <code className="text-sm bg-muted px-2 py-1 rounded">-n</code> line numbers, <code className="text-sm bg-muted px-2 py-1 rounded">-r</code> recursive, <code className="text-sm bg-muted px-2 py-1 rounded">-c</code> count only.</p>

        <Heading3>sort</Heading3>
        <CodeBlock
          language="bash"
          code={`❯ sort -r names.txt
Zara
Mike
Alice`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>System Information</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>whoami</code></td><td className="py-2">Print current user name</td></tr>
              <tr><td className="py-2 pr-4"><code>hostname</code></td><td className="py-2">Print machine name</td></tr>
              <tr><td className="py-2 pr-4"><code>uptime</code></td><td className="py-2">Show system uptime</td></tr>
              <tr><td className="py-2 pr-4"><code>env</code></td><td className="py-2">List all environment variables</td></tr>
              <tr><td className="py-2 pr-4"><code>export VAR=value</code></td><td className="py-2">Set an environment variable</td></tr>
              <tr><td className="py-2 pr-4"><code>unset VAR</code></td><td className="py-2">Remove an environment variable</td></tr>
              <tr><td className="py-2 pr-4"><code>where command</code></td><td className="py-2">Locate all sources (alias, label, builtin, PATH)</td></tr>
              <tr><td className="py-2 pr-4"><code>date [+format]</code></td><td className="py-2">Print current date/time (<code>-u</code> UTC, <code>-I</code> ISO)</td></tr>
            </tbody>
          </table>
        </div>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Utilities</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>echo [text]</code></td><td className="py-2">Print text</td></tr>
              <tr><td className="py-2 pr-4"><code>sleep seconds</code></td><td className="py-2">Pause for N seconds</td></tr>
              <tr><td className="py-2 pr-4"><code>yes [text]</code></td><td className="py-2">Repeat text (default: y)</td></tr>
              <tr><td className="py-2 pr-4"><code>seq [first [inc]] last</code></td><td className="py-2">Print a sequence of numbers</td></tr>
              <tr><td className="py-2 pr-4"><code>true</code></td><td className="py-2">Return success (exit code 0)</td></tr>
              <tr><td className="py-2 pr-4"><code>false</code></td><td className="py-2">Return failure (exit code 1)</td></tr>
              <tr><td className="py-2 pr-4"><code>time command</code></td><td className="py-2">Measure command execution time</td></tr>
              <tr><td className="py-2 pr-4"><code>watch [-n s] command</code></td><td className="py-2">Repeat command every N seconds</td></tr>
              <tr><td className="py-2 pr-4"><code>wget url</code></td><td className="py-2">Download a file from a URL</td></tr>
              <tr><td className="py-2 pr-4"><code>history</code></td><td className="py-2">Show command history</td></tr>
              <tr><td className="py-2 pr-4"><code>clear</code></td><td className="py-2">Clear the screen</td></tr>
              <tr><td className="py-2 pr-4"><code>help</code></td><td className="py-2">Show help with all commands</td></tr>
              <tr><td className="py-2 pr-4"><code>reset</code></td><td className="py-2">Reset the session state</td></tr>
              <tr><td className="py-2 pr-4"><code>exit / quit</code></td><td className="py-2">Exit the shell</td></tr>
            </tbody>
          </table>
        </div>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Session Inspection</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>vars</code></td><td className="py-2">Show all defined variables and their types</td></tr>
              <tr><td className="py-2 pr-4"><code>types</code></td><td className="py-2">Show available types (built-in, user-defined, imported)</td></tr>
              <tr><td className="py-2 pr-4"><code>functions</code></td><td className="py-2">Show defined functions</td></tr>
              <tr><td className="py-2 pr-4"><code>source file</code></td><td className="py-2">Execute a .jz file in the current session</td></tr>
            </tbody>
          </table>
        </div>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Privilege Escalation</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>sudo command</code></td><td className="py-2">Run a command with elevated privileges</td></tr>
              <tr><td className="py-2 pr-4"><code>sudo -s</code></td><td className="py-2">Open an elevated shell (replaces current session)</td></tr>
              <tr><td className="py-2 pr-4"><code>sudo -i</code></td><td className="py-2">Open an elevated login shell (resets to home directory)</td></tr>
            </tbody>
          </table>
        </div>
        <p>
          On Windows, <code className="text-sm bg-muted px-2 py-1 rounded">sudo</code> launches an elevated Jitzu shell process and attaches
          it to the current console. The prompt shows <code className="text-sm bg-muted px-2 py-1 rounded">[sudo]</code> and uses <code className="text-sm bg-muted px-2 py-1 rounded">#</code> instead of <code className="text-sm bg-muted px-2 py-1 rounded">❯</code> when elevated.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
