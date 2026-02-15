import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/pipes-and-redirection")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Pipes & Redirection</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell supports Unix-style pipes and I/O redirection, plus a unique hybrid pipe
          system that lets you chain OS command output into Jitzu functions.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Unix Pipes</Heading2>
        <p>
          Use <code className="text-sm bg-muted px-2 py-1 rounded">|</code> to pass the output of one command as input to another.
          When both sides are OS commands, the shell delegates to the system pipe:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ git log --oneline | head -5
a1b2c3d Add pattern matching
b2c3d4e Fix lexer edge case
c3d4e5f Refactor parser
d4e5f6a Add shell mode
e5f6a7b Initial commit`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Hybrid Pipes</Heading2>
        <p>
          You can pipe OS command output into Jitzu pipe functions. The shell captures
          stdout from the left side and passes it as text to the Jitzu function on the right:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ git log --oneline | first
a1b2c3d Add pattern matching

❯ ls | grep("cs")
Parser.cs
Lexer.cs
Interpreter.cs

❯ git log --oneline | nth(2)
c3d4e5f Refactor parser`}
        />

        <Heading3>Available Pipe Functions</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Function</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>first</code></td><td className="py-2">First line of output</td></tr>
              <tr><td className="py-2 pr-4"><code>last</code></td><td className="py-2">Last line of output</td></tr>
              <tr><td className="py-2 pr-4"><code>nth(n)</code></td><td className="py-2">Nth line (0-indexed)</td></tr>
              <tr><td className="py-2 pr-4"><code>grep("pattern")</code></td><td className="py-2">Filter lines containing pattern</td></tr>
              <tr><td className="py-2 pr-4"><code>head -n N</code></td><td className="py-2">First N lines (default 10)</td></tr>
              <tr><td className="py-2 pr-4"><code>tail -n N</code></td><td className="py-2">Last N lines (default 10)</td></tr>
              <tr><td className="py-2 pr-4"><code>sort [-r]</code></td><td className="py-2">Sort lines alphabetically</td></tr>
              <tr><td className="py-2 pr-4"><code>uniq</code></td><td className="py-2">Remove consecutive duplicate lines</td></tr>
              <tr><td className="py-2 pr-4"><code>wc [-l/-w/-c]</code></td><td className="py-2">Count lines/words/chars</td></tr>
              <tr><td className="py-2 pr-4"><code>tee [-a] file</code></td><td className="py-2">Write to file and pass through</td></tr>
              <tr><td className="py-2 pr-4"><code>more / less</code></td><td className="py-2">View output in pager</td></tr>
              <tr><td className="py-2 pr-4"><code>print</code></td><td className="py-2">Print and pass through</td></tr>
            </tbody>
          </table>
        </div>

        <p>Pipe functions accept both Jitzu-style and shell-style argument syntax:</p>
        <CodeBlock
          language="bash"
          code={`❯ ls | grep("test")      // Jitzu-style parentheses
❯ ls | grep "test"       // shell-style spaces`}
        />

        <Heading3>Chaining Pipe Functions</Heading3>
        <p>Pipe functions can be chained together:</p>
        <CodeBlock
          language="bash"
          code={`❯ git log --oneline | grep("fix") | sort
a1b2c3d Fix login timeout
c3d4e5f Fix parser edge case

❯ ls *.cs | sort | tee filelist.txt`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Builtin Pipes</Heading2>
        <p>
          Built-in commands can also be piped. Their output is captured and fed into the right side:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ diff file1.txt file2.txt | more
❯ cat server.log | grep("ERROR")
❯ find src -ext cs | sort`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Output Redirection</Heading2>
        <p>
          Redirect command output to a file with <code className="text-sm bg-muted px-2 py-1 rounded">{">"}</code> (overwrite) or <code className="text-sm bg-muted px-2 py-1 rounded">{">>"}</code> (append).
          ANSI color codes are automatically stripped from redirected output.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ echo "hello world" > greeting.txt
❯ echo "another line" >> greeting.txt

❯ ls *.cs > filelist.txt

❯ grep -rn "TODO" src/ > todos.txt`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Input Redirection</Heading2>
        <p>
          Use <code className="text-sm bg-muted px-2 py-1 rounded">{"<"}</code> to feed a file's contents as stdin to a command:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ sort < names.txt
Alice
Bob
Charlie`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Tee</Heading2>
        <p>
          The <code className="text-sm bg-muted px-2 py-1 rounded">tee</code> command writes its input to a file and also passes it through
          to stdout, so you can save intermediate results in a pipeline:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ ls | tee filelist.txt
src/
tests/
Program.cs

❯ git log --oneline | tee -a log.txt | first
a1b2c3d Add pattern matching`}
        />
        <p>Use <code className="text-sm bg-muted px-2 py-1 rounded">-a</code> to append instead of overwriting.</p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Command Chaining</Heading2>
        <p>Chain multiple commands together with logical operators:</p>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Operator</th>
                <th className="text-left py-2 font-medium">Behavior</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>&&</code></td><td className="py-2">Run next command only if the previous succeeded</td></tr>
              <tr><td className="py-2 pr-4"><code>||</code></td><td className="py-2">Run next command only if the previous failed</td></tr>
              <tr><td className="py-2 pr-4"><code>;</code></td><td className="py-2">Run next command unconditionally</td></tr>
            </tbody>
          </table>
        </div>

        <CodeBlock
          language="bash"
          code={`❯ mkdir build && cd build
❯ echo hello > file.txt && cat file.txt
hello

❯ cd nonexistent || echo "directory not found"
directory not found

❯ echo step1 ; echo step2 ; echo step3
step1
step2
step3`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Background Execution</Heading2>
        <p>
          Append <code className="text-sm bg-muted px-2 py-1 rounded">&</code> to run a command in the background. See the{" "}
          <a href="/docs/shell/activity-monitor" className="text-primary hover:underline">
            Activity Monitor
          </a>{" "}
          page for more on background job management.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ dotnet build &
[1] 12345

❯ jobs
[1]  Running    dotnet build`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Call Operator</Heading2>
        <p>
          The <code className="text-sm bg-muted px-2 py-1 rounded">&</code> prefix (PowerShell-style call operator) forces a line to
          be treated as an OS command instead of Jitzu code:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ & dotnet build`}
        />
      </section>
      </ScrollReveal>
    </article>
  );
}
